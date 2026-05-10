//Code for PlayersReady (Container)
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
partial class PlayersReady : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("PlayersReady");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named PlayersReady - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new PlayersReady(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(PlayersReady)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("PlayersReady", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public enum Ready
    {
        allReady,
        notReady,
    }

    Ready? _readyState;
    public Ready? ReadyState
    {
        get => _readyState;
        set
        {
            _readyState = value;
            if(value != null)
            {
                if(Visual.Categories.ContainsKey("Ready"))
                {
                    var category = Visual.Categories["Ready"];
                    var state = category.States.Find(item => item.Name == value.ToString());
                    this.Visual.ApplyState(state);
                }
                else
                {
                    var category = ((global::Gum.DataTypes.ElementSave)this.Visual.Tag).Categories.FirstOrDefault(item => item.Name == "Ready");
                    var state = category.States.Find(item => item.Name == value.ToString());
                    this.Visual.ApplyState(state);
                }
            }
        }
    }
    public SpriteRuntime BluePlayer { get; protected set; }
    public SpriteRuntime BrownPlayer { get; protected set; }
    public SpriteRuntime YellowPlayer { get; protected set; }
    public SpriteRuntime RedPlayer { get; protected set; }
    public TextRuntime TextInstance { get; protected set; }
    public ContainerRuntime ContainerInstance { get; protected set; }

    public PlayersReady(InteractiveGue visual) : base(visual)
    {
    }
    public PlayersReady()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        BluePlayer = this.Visual?.GetGraphicalUiElementByName("BluePlayer") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        BrownPlayer = this.Visual?.GetGraphicalUiElementByName("BrownPlayer") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        YellowPlayer = this.Visual?.GetGraphicalUiElementByName("YellowPlayer") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        RedPlayer = this.Visual?.GetGraphicalUiElementByName("RedPlayer") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        TextInstance = this.Visual?.GetGraphicalUiElementByName("TextInstance") as global::MonoGameGum.GueDeriving.TextRuntime;
        ContainerInstance = this.Visual?.GetGraphicalUiElementByName("ContainerInstance") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
