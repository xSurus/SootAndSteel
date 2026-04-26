//Code for MainMenuScreen
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
namespace Gamelab.Screens;
partial class MainMenuScreen : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("MainMenuScreen");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named MainMenuScreen - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new MainMenuScreen(visual);
            visual.Width = 0;
            visual.WidthUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
            visual.Height = 0;
            visual.HeightUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(MainMenuScreen)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("MainMenuScreen", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public MainMenuButton ContinueButton { get; protected set; }
    public MainMenuButton NewGameButton { get; protected set; }
    public MainMenuButton OptionsButton { get; protected set; }
    public MainMenuButton QuitButton { get; protected set; }
    public SpriteRuntime Background { get; protected set; }
    public ContainerRuntime MenuItemContainer { get; protected set; }

    public MainMenuScreen(InteractiveGue visual) : base(visual)
    {
    }
    public MainMenuScreen()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        ContinueButton = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<MainMenuButton>(this.Visual,"ContinueButton");
        NewGameButton = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<MainMenuButton>(this.Visual,"NewGameButton");
        OptionsButton = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<MainMenuButton>(this.Visual,"OptionsButton");
        QuitButton = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<MainMenuButton>(this.Visual,"QuitButton");
        Background = this.Visual?.GetGraphicalUiElementByName("Background") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        MenuItemContainer = this.Visual?.GetGraphicalUiElementByName("MenuItemContainer") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
