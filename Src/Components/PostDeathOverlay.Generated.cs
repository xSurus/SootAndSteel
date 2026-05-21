//Code for PostDeathOverlay (Container)
using Gamelab.Components;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;
using GumRuntime;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;
using System.Linq;
namespace Gamelab.Components;
partial class PostDeathOverlay : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("PostDeathOverlay");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named PostDeathOverlay - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new PostDeathOverlay(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(PostDeathOverlay)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("PostDeathOverlay", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public PostDeathStatItem EnemiesDefeatedStat { get; protected set; }
    public PostDeathStatItem DistanceTravelledStat { get; protected set; }
    public PostDeathStatItem StagesDefeatedStat { get; protected set; }
    public PostDeathStatItem UpgradesBoughtStat { get; protected set; }
    public ColoredRectangleRuntime Vigniette { get; protected set; }
    public NineSliceRuntime Background { get; protected set; }
    public TextRuntime OfficeText { get; protected set; }
    public TextRuntime IncidentTitleDetail { get; protected set; }
    public TextRuntime EnemiesDefeated { get; protected set; }
    public TextRuntime DistanceTravelled { get; protected set; }
    public TextRuntime StagesDefeated { get; protected set; }
    public TextRuntime Upgrades_Bought { get; protected set; }
    public TextRuntime FinishStageText { get; protected set; }
    public RectangleRuntime SeperatorTitle { get; protected set; }
    public TextRuntime SeperatorTop { get; protected set; }
    public TextRuntime CauseOfFailureTitle { get; protected set; }
    public TextRuntime CauseOfFailureDescription { get; protected set; }
    public TextRuntime SeperatorBottom { get; protected set; }
    public SpriteRuntime FileClosedStamp { get; protected set; }
    public SpriteRuntime hole1 { get; protected set; }
    public ButtonWithIcon ButtonWithIconInstance { get; protected set; }
    public SpriteRuntime hole2 { get; protected set; }

    public string ButtonWithIconInstanceButtonIcon
    {
        set => ButtonWithIconInstance.ButtonIcon = value;
    }

    public string CauseOfFailureDescriptionText
    {
        get => CauseOfFailureDescription.Text;
        set => CauseOfFailureDescription.Text = value;
    }

    public string DistanceTravelledStatStatText
    {
        get => DistanceTravelledStat.StatText;
        set => DistanceTravelledStat.StatText = value;
    }

    public string EnemiesDefeatedStatStatText
    {
        get => EnemiesDefeatedStat.StatText;
        set => EnemiesDefeatedStat.StatText = value;
    }

    public string FinishStageTextText
    {
        get => FinishStageText.Text;
        set => FinishStageText.Text = value;
    }

    public string StagesDefeatedStatStatText
    {
        get => StagesDefeatedStat.StatText;
        set => StagesDefeatedStat.StatText = value;
    }

    public string UpgradesBoughtStatStatText
    {
        get => UpgradesBoughtStat.StatText;
        set => UpgradesBoughtStat.StatText = value;
    }

    public PostDeathOverlay(InteractiveGue visual) : base(visual)
    {
    }
    public PostDeathOverlay()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        EnemiesDefeatedStat = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<PostDeathStatItem>(this.Visual,"EnemiesDefeatedStat");
        DistanceTravelledStat = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<PostDeathStatItem>(this.Visual,"DistanceTravelledStat");
        StagesDefeatedStat = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<PostDeathStatItem>(this.Visual,"StagesDefeatedStat");
        UpgradesBoughtStat = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<PostDeathStatItem>(this.Visual,"UpgradesBoughtStat");
        Vigniette = this.Visual?.GetGraphicalUiElementByName("Vigniette") as global::MonoGameGum.GueDeriving.ColoredRectangleRuntime;
        Background = this.Visual?.GetGraphicalUiElementByName("Background") as global::MonoGameGum.GueDeriving.NineSliceRuntime;
        OfficeText = this.Visual?.GetGraphicalUiElementByName("OfficeText") as global::MonoGameGum.GueDeriving.TextRuntime;
        IncidentTitleDetail = this.Visual?.GetGraphicalUiElementByName("IncidentTitleDetail") as global::MonoGameGum.GueDeriving.TextRuntime;
        EnemiesDefeated = this.Visual?.GetGraphicalUiElementByName("EnemiesDefeated") as global::MonoGameGum.GueDeriving.TextRuntime;
        DistanceTravelled = this.Visual?.GetGraphicalUiElementByName("DistanceTravelled") as global::MonoGameGum.GueDeriving.TextRuntime;
        StagesDefeated = this.Visual?.GetGraphicalUiElementByName("StagesDefeated") as global::MonoGameGum.GueDeriving.TextRuntime;
        Upgrades_Bought = this.Visual?.GetGraphicalUiElementByName("Upgrades Bought") as global::MonoGameGum.GueDeriving.TextRuntime;
        FinishStageText = this.Visual?.GetGraphicalUiElementByName("FinishStageText") as global::MonoGameGum.GueDeriving.TextRuntime;
        SeperatorTitle = this.Visual?.GetGraphicalUiElementByName("SeperatorTitle") as global::MonoGameGum.GueDeriving.RectangleRuntime;
        SeperatorTop = this.Visual?.GetGraphicalUiElementByName("SeperatorTop") as global::MonoGameGum.GueDeriving.TextRuntime;
        CauseOfFailureTitle = this.Visual?.GetGraphicalUiElementByName("CauseOfFailureTitle") as global::MonoGameGum.GueDeriving.TextRuntime;
        CauseOfFailureDescription = this.Visual?.GetGraphicalUiElementByName("CauseOfFailureDescription") as global::MonoGameGum.GueDeriving.TextRuntime;
        SeperatorBottom = this.Visual?.GetGraphicalUiElementByName("SeperatorBottom") as global::MonoGameGum.GueDeriving.TextRuntime;
        FileClosedStamp = this.Visual?.GetGraphicalUiElementByName("FileClosedStamp") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        hole1 = this.Visual?.GetGraphicalUiElementByName("hole1") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        ButtonWithIconInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonWithIcon>(this.Visual,"ButtonWithIconInstance");
        hole2 = this.Visual?.GetGraphicalUiElementByName("hole2") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
