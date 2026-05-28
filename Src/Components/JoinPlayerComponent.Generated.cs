//Code for JoinPlayerComponent (Container)
using Gamelab.Components;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.StateAnimation.Runtime;
using Gum.Wireframe;
using GumRuntime;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;
using System.Linq;
namespace Gamelab.Components;
partial class JoinPlayerComponent : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("JoinPlayerComponent");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named JoinPlayerComponent - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new JoinPlayerComponent(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(JoinPlayerComponent)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("JoinPlayerComponent", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public enum Joined
    {
        joined1,
        empty,
        joined2,
        joined3,
        joined4,
    }

    Joined? _joinedState;
    public Joined? JoinedState
    {
        get => _joinedState;
        set
        {
            _joinedState = value;
            if(value != null)
            {
                if(Visual.Categories.ContainsKey("Joined"))
                {
                    var category = Visual.Categories["Joined"];
                    var state = category.States.Find(item => item.Name == value.ToString());
                    this.Visual.ApplyState(state);
                }
                else
                {
                    var category = ((global::Gum.DataTypes.ElementSave)this.Visual.Tag).Categories.FirstOrDefault(item => item.Name == "Joined");
                    var state = category.States.Find(item => item.Name == value.ToString());
                    this.Visual.ApplyState(state);
                }
            }
        }
    }
    public SpriteRuntime PlayerFigure { get; protected set; }
    public ButtonWithIcon Join { get; protected set; }
    public ContainerRuntime Player { get; protected set; }

    public int JoinText
    {
        get;
        set;
    }
    public string SpriteInstanceSourceFile
    {
        set => PlayerFigure.SourceFileName = value;
    }

    public JoinPlayerComponent(InteractiveGue visual) : base(visual)
    {
    }
    public JoinPlayerComponent()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        PlayerFigure = this.Visual?.GetGraphicalUiElementByName("PlayerFigure") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        Join = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonWithIcon>(this.Visual,"Join");
        Player = this.Visual?.GetGraphicalUiElementByName("Player") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
