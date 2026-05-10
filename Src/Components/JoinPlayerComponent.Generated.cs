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
    public enum MovingPlayer
    {
        Frame0Joined,
        Frame1Joined,
        Frame3Joined,
        Frame4Joined,
        Frame0Empty,
        Frame1Empty,
        Frame2Empty,
        Frame3Empty,
    }

    MovingPlayer? _movingPlayerState;
    public MovingPlayer? MovingPlayerState
    {
        get => _movingPlayerState;
        set
        {
            _movingPlayerState = value;
            if(value != null)
            {
                if(Visual.Categories.ContainsKey("MovingPlayer"))
                {
                    var category = Visual.Categories["MovingPlayer"];
                    var state = category.States.Find(item => item.Name == value.ToString());
                    this.Visual.ApplyState(state);
                }
                else
                {
                    var category = ((global::Gum.DataTypes.ElementSave)this.Visual.Tag).Categories.FirstOrDefault(item => item.Name == "MovingPlayer");
                    var state = category.States.Find(item => item.Name == value.ToString());
                    this.Visual.ApplyState(state);
                }
            }
        }
    }
    public SpriteRuntime PlayerFigure { get; protected set; }
    public ButtonWithIcon Join { get; protected set; }
    public ContainerRuntime Player { get; protected set; }


    #region Animation Fields
    public AnimationRuntime PlayerJoinedAnimation {get; protected set;}
    public AnimationRuntime PlayerEmptyAnimation {get; protected set;}
    #endregion
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
        PlayerJoinedAnimation = this.Visual.GetAnimation("PlayerJoinedAnimation");
        PlayerEmptyAnimation = this.Visual.GetAnimation("PlayerEmptyAnimation");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
