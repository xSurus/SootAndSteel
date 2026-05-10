using Gamelab;
using Gamelab.Components;
using Gamelab.UI;
using Microsoft.Xna.Framework;
using MonoGameGum;

namespace Gamelab.Dialogue;

public class DialogueOverlay
{
    private const float CharactersPerSecond = 48f;
    private const int InteractionHintFontSize = 17;
    private const float BottomScreenMarginPixels = 220f;
    private const float PassiveTutorialBottomGapPixels = 20f;
    private const float PassiveTutorialMinTopFraction = 0.46f;

    private const float HeadBubbleLiftPx = 10f;
    private static readonly Vector2 AnchorBubbleScreenNudgePx = new(80f, 0f);

    private readonly DialogBubble bubble;
    private string fullBodyText = "";
    private int revealedCharCount;
    private float revealCarryOver;

    private bool hasWorldAnchor;
    private Vector2 anchorWorldPixels;
    private Matrix cachedCameraMatrix = Matrix.Identity;

    private bool passiveTutorialChromeHidden;

    public DialogueLine CurrentLine { get; private set; }

    public DialogueOverlay()
    {
        bubble = new DialogBubble();
        bubble.AddToRoot();
        ConfigureInteractionRow();
        bubble.Tail.Visible = false;
        bubble.Visual.Visible = false;
    }

    public void SetWorldAnchor(Vector2? worldPixels)
    {
        hasWorldAnchor = worldPixels.HasValue;
        anchorWorldPixels = worldPixels ?? default;
    }

    public void SyncFollowCamera(Matrix cameraViewMatrix)
    {
        if (CurrentLine == null && !bubble.Visual.Visible) return;
        cachedCameraMatrix = cameraViewMatrix;
        RefreshLayoutAndPosition();
    }

    public bool IsLineFullyRevealed =>
        CurrentLine == null || revealedCharCount >= fullBodyText.Length;

    public void Update(GameTime gameTime)
    {
        if (CurrentLine == null || bubble.Visual.Visible == false) return;
        if (passiveTutorialChromeHidden) return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (revealedCharCount >= fullBodyText.Length) return;

        revealCarryOver += dt * CharactersPerSecond;
        int add = (int)revealCarryOver;
        if (add <= 0) return;

        revealCarryOver -= add;
        revealedCharCount = System.Math.Min(fullBodyText.Length, revealedCharCount + add);
        bubble.DialogText = fullBodyText[..revealedCharCount];
        SyncDismissButtonInteractivity();
        RefreshLayoutAndPosition();
    }

    public void Show(DialogueLine line)
    {
        passiveTutorialChromeHidden = false;
        SetInteractionChromeVisible(true);

        CurrentLine = line;
        fullBodyText = line.Text ?? "";
        revealedCharCount = 0;
        revealCarryOver = 0f;

        bubble.DialogName.Text = line.SpeakerName;
        bubble.DialogText = "";
        ConfigureInteractionRow();
        bubble.Tail.Visible = hasWorldAnchor;
        bubble.Visual.Visible = true;
        RefreshLayoutAndPosition();
    }

    public void ShowDecision(DialogueLine line, string leftButtonText, string rightButtonText)
    {
        // Keep decision prompts at the same lower screen position as passive hints,
        // but still show interaction buttons.
        passiveTutorialChromeHidden = true;
        SetInteractionChromeVisible(true);

        CurrentLine = line;
        fullBodyText = line.Text ?? "";
        revealedCharCount = fullBodyText.Length;
        revealCarryOver = 0f;
        hasWorldAnchor = false;

        bubble.DialogName.Text = line.SpeakerName;
        bubble.DialogText = fullBodyText;
        ConfigureInteractionRow(leftButtonText, rightButtonText);
        bubble.Tail.Visible = false;
        bubble.Visual.Visible = true;
        RefreshLayoutAndPosition();
    }

    public void ShowPassive(DialogueLine line)
    {
        passiveTutorialChromeHidden = true;
        SetInteractionChromeVisible(false);

        CurrentLine = line;
        fullBodyText = line.Text ?? "";
        revealedCharCount = fullBodyText.Length;
        revealCarryOver = 0f;
        hasWorldAnchor = false;

        bubble.DialogName.Text = line.SpeakerName;
        bubble.DialogText = fullBodyText;
        bubble.Tail.Visible = false;
        bubble.Visual.Visible = true;
        RefreshLayoutAndPosition();
    }

    public void Hide()
    {
        passiveTutorialChromeHidden = false;
        SetInteractionChromeVisible(true);

        CurrentLine = null;
        fullBodyText = "";
        revealedCharCount = 0;
        hasWorldAnchor = false;
        bubble.Tail.Visible = false;
        bubble.Visual.Visible = false;
    }

    public void CompleteReveal()
    {
        if (CurrentLine == null) return;
        revealedCharCount = fullBodyText.Length;
        bubble.DialogText = fullBodyText;
        SyncDismissButtonInteractivity();
        RefreshLayoutAndPosition();
    }

    private void SetInteractionChromeVisible(bool visible)
    {
        if (bubble.InteractionsDialog != null)
            bubble.InteractionsDialog.Visible = visible;

        if (bubble.ButtonWithIconInstance != null)
            bubble.ButtonWithIconInstance.Visual.Visible = visible;
        if (bubble.ButtonWithIconInstance1 != null)
            bubble.ButtonWithIconInstance1.Visual.Visible = visible;
    }

    private void ConfigureInteractionRow(string leftText = null, string rightText = null)
    {
        if (bubble.ButtonWithIconInstance is { } left)
        {
            left.Visual.Visible = true;
            left.TextInstance.FontSize = InteractionHintFontSize;
            XboxButtonGlyphs.ApplyFaceButton(left, XboxButtonAtlas.Face.Y);
            left.ButtonText = leftText ?? "Dismiss";
            SyncDismissButtonInteractivity();
        }

        if (bubble.ButtonWithIconInstance1 is not { } right) return;

        right.Visual.Visible = true;
        right.TextInstance.FontSize = InteractionHintFontSize;
        XboxButtonGlyphs.ApplyFaceButton(right, XboxButtonAtlas.Face.X);
        right.ButtonText = rightText ?? (MoreLinesQueued ? "Continue" : "Close");
    }

    public bool IsDismissInputAllowed => CurrentLine != null && IsLineFullyRevealed;

    private void SyncDismissButtonInteractivity()
    {
    }

    public bool MoreLinesQueued { get; set; }

    private void RefreshLayoutAndPosition()
    {
        float canvasW = GumService.Default.CanvasWidth;
        float canvasH = GumService.Default.CanvasHeight;

        GumService.Default.Root.UpdateLayout();

        float bubbleW = bubble.Visual.GetAbsoluteWidth();
        float bubbleH = bubble.Visual.GetAbsoluteHeight();
        if (bubbleW < 1f) bubbleW = bubble.Visual.Width;
        if (bubbleH < 1f) bubbleH = bubble.Visual.Height;

        if (hasWorldAnchor)
        {
            Vector2 screenPos = Vector2.Transform(anchorWorldPixels, cachedCameraMatrix);
            Vector2 gum = ConvertScreenToGumCanvas(screenPos);

            float x = gum.X - bubbleW / 2f + AnchorBubbleScreenNudgePx.X;
            float y = gum.Y - bubbleH - HeadBubbleLiftPx + AnchorBubbleScreenNudgePx.Y;

            x = MathHelper.Clamp(x, 8f, System.Math.Max(8f, canvasW - bubbleW - 8f));
            y = MathHelper.Clamp(y, 8f, System.Math.Max(8f, canvasH - bubbleH - 8f));

            bubble.Visual.X = x;
            bubble.Visual.Y = y;
        }
        else
        {
            bubble.Visual.X = (canvasW - bubbleW) / 2f;

            if (passiveTutorialChromeHidden)
            {
                float minTop = canvasH * PassiveTutorialMinTopFraction;
                float y = canvasH - bubbleH - PassiveTutorialBottomGapPixels;
                if (y < minTop)
                    y = minTop;
                if (y + bubbleH > canvasH - PassiveTutorialBottomGapPixels)
                    y = canvasH - bubbleH - PassiveTutorialBottomGapPixels;
                bubble.Visual.Y = y;
            }
            else
                bubble.Visual.Y = canvasH - bubbleH - BottomScreenMarginPixels;
        }
    }

    private static Vector2 ConvertScreenToGumCanvas(Vector2 screenPos)
    {
        var game = GamelabGame.Instance;
        float scale = game.GumViewportScale;
        if (scale <= 0f)
        {
            return screenPos;
        }

        var viewport = game.GraphicsDevice.Viewport;
        float cw = GumService.Default.CanvasWidth;
        float ch = GumService.Default.CanvasHeight;
        if (cw <= 0f || ch <= 0f)
        {
            return screenPos / scale;
        }

        float viewportOffsetX = (viewport.Width - cw * scale) * 0.5f;
        float viewportOffsetY = (viewport.Height - ch * scale) * 0.5f;

        return new Vector2(
            (screenPos.X - viewportOffsetX) / scale,
            (screenPos.Y - viewportOffsetY) / scale);
    }
}
