using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Resources;

namespace FramePFX.Editors.Factories {
    public class ResourceTypeFactory : ReflectiveObjectFactory<BaseResource> {
        public static ResourceTypeFactory Instance { get; } = new ResourceTypeFactory();

        private ResourceTypeFactory() {
            this.RegisterType("r_group", typeof(ResourceFolder));
            this.RegisterType("r_argb", typeof(ResourceColour));
            this.RegisterType("r_img", typeof(ResourceImage));
            this.RegisterType("r_txt", typeof(ResourceTextStyle));
        }

        public BaseResource NewResource(string id) {
            return base.NewInstance(id);
        }
    }
}