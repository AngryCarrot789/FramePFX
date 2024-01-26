using FramePFX.Editors.Timelines.Clips;

namespace FramePFX.Editors.Factories {
    public class ClipFactory : ReflectiveObjectFactory<Clip> {
        public static ClipFactory Instance { get; } = new ClipFactory();

        private ClipFactory() {
            // no need to register the base class, since you can't
            // create an instance of an abstract class
            // this.RegisterType("clip_vid", typeof(VideoClip));
            this.RegisterType("vc_shape", typeof(VideoClipShape));
            this.RegisterType("vc_image", typeof(ImageVideoClip));
        }

        public Clip NewClip(string id) {
            return base.NewInstance(id);
        }
    }
}