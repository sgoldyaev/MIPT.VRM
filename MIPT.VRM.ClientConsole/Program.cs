using org.ogre;

namespace MIPT.VRM.ClientConsole
{
    public class KeyListener : InputListener
    {
        ApplicationContext ctx;

        public KeyListener(ApplicationContext ctx)
        {
            this.ctx = ctx;
        }

        public override bool keyPressed(KeyboardEvent evt)
        {
            if (evt.keysym.sym == 27)
                this.ctx.getRoot().queueEndRendering();
            return true;
        }
    }

    public class AppContext : ApplicationContext
    {
        private readonly InputListener listener;

        public AppContext()
        {
            listener = new KeyListener(this);
        }

        public override void setup()
        {
            base.setup();
            addInputListener(listener);

            var root = getRoot();
            var scnMgr = root.createSceneManager();

            var shadergen = ShaderGenerator.getSingleton();
            shadergen.addSceneManager(scnMgr); // must be done before we do anything with the scene

            scnMgr.setAmbientLight(new ColourValue(.1f, .1f, .1f));

            var light = scnMgr.createLight("MainLight");
            var lightnode = scnMgr.getRootSceneNode().createChildSceneNode();
            lightnode.setPosition(0f, 10f, 15f);
            lightnode.attachObject(light);

            var cam = scnMgr.createCamera("myCam");
            cam.setAutoAspectRatio(true);
            cam.setNearClipDistance(5);
            var camnode = scnMgr.getRootSceneNode().createChildSceneNode();
            camnode.attachObject(cam);

            var camman = new CameraMan(camnode);
            camman.setStyle(CameraStyle.CS_ORBIT);
            camman.setYawPitchDist(new Radian(0), new Radian(0.3f), 15f);
            addInputListener(camman);

            var vp = getRenderWindow().addViewport(cam);
            vp.setBackgroundColour(new ColourValue(.3f, .3f, .3f));

            // var ent = scnMgr.createEntity("Assets/robot.mesh");
            // var node = scnMgr.getRootSceneNode().createChildSceneNode();
            // node.attachObject(ent);
        }
    }
    
    internal class Program
    {
        public static void Main(string[] args)
        {
            var app = new AppContext();
            app.initApp();
            app.getRoot().startRendering();
            app.closeApp();
        }
    }
}
