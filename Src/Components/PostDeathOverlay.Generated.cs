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
    public RectangleRuntime SeparatorTitleLine { get; protected set; }
    public ContainerRuntime SeparatorTitle { get; protected set; }
    public ContainerRuntime EnemiesDefeatedLine { get; protected set; }
    public ContainerRuntime DistanceTravelledLine { get; protected set; }
    public TextRuntime EnemiesDefeated { get; protected set; }
    public PostDeathStatItem EnemiesDefeatedStatText { get; protected set; }
    public TextRuntime DistanceTravelled { get; protected set; }
    public PostDeathStatItem DistanceTravelledStatText { get; protected set; }
    public TextRuntime StagesDefeated { get; protected set; }
    public PostDeathStatItem StagesDefeatedStatText { get; protected set; }
    public TextRuntime UpgradesBought { get; protected set; }
    public PostDeathStatItem UpgradesBoughtStatText { get; protected set; }
    public TextRuntime OfficeText { get; protected set; }
    public TextRuntime FinishStageText { get; protected set; }
    public ContainerRuntime Title { get; protected set; }
    public ContainerRuntime MainBox { get; protected set; }
    public NineSliceRuntime Background { get; protected set; }
    public ContainerRuntime Content { get; protected set; }
    public SpriteRuntime FileClosedStamp { get; protected set; }
    public SpriteRuntime hole2 { get; protected set; }
    public SpriteRuntime hole1 { get; protected set; }
    public TextRuntime IncidentTitleDetail { get; protected set; }
    public ColoredRectangleRuntime Vignette { get; protected set; }
    public ContainerRuntime Paper { get; protected set; }
    public ContainerRuntime Stats { get; protected set; }
    public TextRuntime SeparatorTop { get; protected set; }
    public TextRuntime CauseOfFailureTitle { get; protected set; }
    public TextRuntime CauseOfFailureDescription { get; protected set; }
    public TextRuntime SeparatorBottom { get; protected set; }
    public ButtonWithIcon ButtonWithIconInstance { get; protected set; }
    public ContainerRuntime Column1 { get; protected set; }
    public ContainerRuntime Column2 { get; protected set; }
    public ContainerRuntime StagesDefeatedLine { get; protected set; }
    public ContainerRuntime UpgradesBoughtLine { get; protected set; }

    public string FinishStageTextText
    {
        get => FinishStageText.Text;
        set => FinishStageText.Text = value;
    }

    public PostDeathOverlay(InteractiveGue visual) : base(visual)
    {
    }
    public PostDeathOverlay()
    {
    }

    public void SetCauseOfFailure(string text)
    {
        CauseOfFailureDescription.Text = text;
    }
    
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        SeparatorTitleLine = this.Visual?.GetGraphicalUiElementByName("SeparatorTitleLine") as global::MonoGameGum.GueDeriving.RectangleRuntime;
        SeparatorTitle = this.Visual?.GetGraphicalUiElementByName("SeparatorTitle") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        EnemiesDefeatedLine = this.Visual?.GetGraphicalUiElementByName("EnemiesDefeatedLine") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        DistanceTravelledLine = this.Visual?.GetGraphicalUiElementByName("DistanceTravelledLine") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        EnemiesDefeated = this.Visual?.GetGraphicalUiElementByName("EnemiesDefeated") as global::MonoGameGum.GueDeriving.TextRuntime;
        EnemiesDefeatedStatText = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<PostDeathStatItem>(this.Visual,"EnemiesDefeatedStatText");
        DistanceTravelled = this.Visual?.GetGraphicalUiElementByName("DistanceTravelled") as global::MonoGameGum.GueDeriving.TextRuntime;
        DistanceTravelledStatText = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<PostDeathStatItem>(this.Visual,"DistanceTravelledStatText");
        StagesDefeated = this.Visual?.GetGraphicalUiElementByName("StagesDefeated") as global::MonoGameGum.GueDeriving.TextRuntime;
        StagesDefeatedStatText = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<PostDeathStatItem>(this.Visual,"StagesDefeatedStatText");
        UpgradesBought = this.Visual?.GetGraphicalUiElementByName("UpgradesBought") as global::MonoGameGum.GueDeriving.TextRuntime;
        UpgradesBoughtStatText = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<PostDeathStatItem>(this.Visual,"UpgradesBoughtStatText");
        OfficeText = this.Visual?.GetGraphicalUiElementByName("OfficeText") as global::MonoGameGum.GueDeriving.TextRuntime;
        FinishStageText = this.Visual?.GetGraphicalUiElementByName("FinishStageText") as global::MonoGameGum.GueDeriving.TextRuntime;
        Title = this.Visual?.GetGraphicalUiElementByName("Title") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        MainBox = this.Visual?.GetGraphicalUiElementByName("MainBox") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        Background = this.Visual?.GetGraphicalUiElementByName("Background") as global::MonoGameGum.GueDeriving.NineSliceRuntime;
        Content = this.Visual?.GetGraphicalUiElementByName("Content") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        FileClosedStamp = this.Visual?.GetGraphicalUiElementByName("FileClosedStamp") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        hole2 = this.Visual?.GetGraphicalUiElementByName("hole2") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        hole1 = this.Visual?.GetGraphicalUiElementByName("hole1") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        IncidentTitleDetail = this.Visual?.GetGraphicalUiElementByName("IncidentTitleDetail") as global::MonoGameGum.GueDeriving.TextRuntime;
        Vignette = this.Visual?.GetGraphicalUiElementByName("Vignette") as global::MonoGameGum.GueDeriving.ColoredRectangleRuntime;
        Paper = this.Visual?.GetGraphicalUiElementByName("Paper") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        Stats = this.Visual?.GetGraphicalUiElementByName("Stats") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        SeparatorTop = this.Visual?.GetGraphicalUiElementByName("SeparatorTop") as global::MonoGameGum.GueDeriving.TextRuntime;
        CauseOfFailureTitle = this.Visual?.GetGraphicalUiElementByName("CauseOfFailureTitle") as global::MonoGameGum.GueDeriving.TextRuntime;
        CauseOfFailureDescription = this.Visual?.GetGraphicalUiElementByName("CauseOfFailureDescription") as global::MonoGameGum.GueDeriving.TextRuntime;
        SeparatorBottom = this.Visual?.GetGraphicalUiElementByName("SeparatorBottom") as global::MonoGameGum.GueDeriving.TextRuntime;
        ButtonWithIconInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonWithIcon>(this.Visual,"ButtonWithIconInstance");
        Column1 = this.Visual?.GetGraphicalUiElementByName("Column1") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        Column2 = this.Visual?.GetGraphicalUiElementByName("Column2") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        StagesDefeatedLine = this.Visual?.GetGraphicalUiElementByName("StagesDefeatedLine") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        UpgradesBoughtLine = this.Visual?.GetGraphicalUiElementByName("UpgradesBoughtLine") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
