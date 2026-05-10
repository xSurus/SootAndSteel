using System;
using System.Collections.Generic;
using Gamelab.Players;
using Gamelab.Utils;
using Microsoft.Xna.Framework;

namespace Gamelab.Dialogue;

public class DialogueManager : IDialogueService
{
    private static readonly Logger logger = new("Dialogue");

    public event Action DialogueEnded;

    private readonly DialogueOverlay overlay;
    private readonly Func<Matrix> getCameraViewMatrix;

    private bool passiveTutorialGuidanceActive;

    public bool IsActive => overlay.CurrentLine != null && !passiveTutorialGuidanceActive;

    public DialogueManager(DialogueOverlay overlay, Func<Matrix> getCameraViewMatrix)
    {
        this.overlay = overlay;
        this.getCameraViewMatrix = getCameraViewMatrix;
    }

    public void SetTutorialGuidance(DialogueLine line)
    {
        passiveTutorialGuidanceActive = true;
        overlay.SetWorldAnchor(null);
        overlay.ShowPassive(line);
    }

    public void ClearTutorialGuidance()
    {
        if (!passiveTutorialGuidanceActive) return;
        passiveTutorialGuidanceActive = false;
        overlay.Hide();
        DialogueEnded?.Invoke();
        logger.Info("Tutorial guidance cleared");
    }

    public void Update(GameTime gameTime, IReadOnlyList<PlayerConfiguration> joinedPlayers)
    {
        if (!passiveTutorialGuidanceActive) return;

        overlay.SyncFollowCamera(getCameraViewMatrix());
        overlay.Update(gameTime);
    }
}
