//Code for DialogBubble (Container)
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
partial class DialogBubble : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("DialogBubble");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named DialogBubble - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new DialogBubble(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(DialogBubble)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("DialogBubble", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public ColoredRectangleRuntime Shadow { get; protected set; }
    public TextRuntime Dialog { get; protected set; }
    public RectangleRuntime Seperator1 { get; protected set; }
    public ContainerRuntime InteractionsDialog { get; protected set; }
    public ContainerRuntime ContainerInstance { get; protected set; }
    public NineSliceRuntime BackgroundParchment { get; protected set; }
    public ContainerRuntime TextinteractionWrapper { get; protected set; }
    public SpriteRuntime Tail { get; protected set; }
    public TextRuntime DialogName { get; protected set; }
    public NineSliceRuntime Header { get; protected set; }
    public ButtonWithIcon ButtonWithIconInstance { get; protected set; }
    public ButtonWithIcon ButtonWithIconInstance1 { get; protected set; }
    public ContainerRuntime Wrapper { get; protected set; }

    public string DialogText
    {
        get => Dialog.Text;
        set => Dialog.Text = value;
    }

    public DialogBubble(InteractiveGue visual) : base(visual)
    {
    }
    public DialogBubble()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        Shadow = this.Visual?.GetGraphicalUiElementByName("Shadow") as global::MonoGameGum.GueDeriving.ColoredRectangleRuntime;
        Dialog = this.Visual?.GetGraphicalUiElementByName("Dialog") as global::MonoGameGum.GueDeriving.TextRuntime;
        Seperator1 = this.Visual?.GetGraphicalUiElementByName("Seperator1") as global::MonoGameGum.GueDeriving.RectangleRuntime;
        InteractionsDialog = this.Visual?.GetGraphicalUiElementByName("InteractionsDialog") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        ContainerInstance = this.Visual?.GetGraphicalUiElementByName("ContainerInstance") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        BackgroundParchment = this.Visual?.GetGraphicalUiElementByName("BackgroundParchment") as global::MonoGameGum.GueDeriving.NineSliceRuntime;
        TextinteractionWrapper = this.Visual?.GetGraphicalUiElementByName("TextinteractionWrapper") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        Tail = this.Visual?.GetGraphicalUiElementByName("Tail") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        DialogName = this.Visual?.GetGraphicalUiElementByName("DialogName") as global::MonoGameGum.GueDeriving.TextRuntime;
        Header = this.Visual?.GetGraphicalUiElementByName("Header") as global::MonoGameGum.GueDeriving.NineSliceRuntime;
        ButtonWithIconInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonWithIcon>(this.Visual,"ButtonWithIconInstance");
        ButtonWithIconInstance1 = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonWithIcon>(this.Visual,"ButtonWithIconInstance1");
        Wrapper = this.Visual?.GetGraphicalUiElementByName("Wrapper") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
