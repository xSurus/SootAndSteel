//Code for JoinScreen
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
partial class JoinScreen : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("JoinScreen");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named JoinScreen - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new JoinScreen(visual);
            visual.Width = 0;
            visual.WidthUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
            visual.Height = 0;
            visual.HeightUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(JoinScreen)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("JoinScreen", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public JoinPlayerComponent First_Player { get; protected set; }
    public JoinPlayerComponent Second_Player { get; protected set; }
    public JoinPlayerComponent Third_Player { get; protected set; }
    public JoinPlayerComponent Fourth_Player { get; protected set; }
    public ContainerRuntime PlayerFigureContainer { get; protected set; }
    public TextRuntime JoinScreenTitle { get; protected set; }

    public string JoinScreenTitleText
    {
        get => JoinScreenTitle.Text;
        set => JoinScreenTitle.Text = value;
    }

    public JoinScreen(InteractiveGue visual) : base(visual)
    {
    }
    public JoinScreen()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        First_Player = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<JoinPlayerComponent>(this.Visual,"First Player");
        Second_Player = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<JoinPlayerComponent>(this.Visual,"Second Player");
        Third_Player = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<JoinPlayerComponent>(this.Visual,"Third Player");
        Fourth_Player = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<JoinPlayerComponent>(this.Visual,"Fourth Player");
        PlayerFigureContainer = this.Visual?.GetGraphicalUiElementByName("PlayerFigureContainer") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        JoinScreenTitle = this.Visual?.GetGraphicalUiElementByName("JoinScreenTitle") as global::MonoGameGum.GueDeriving.TextRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
