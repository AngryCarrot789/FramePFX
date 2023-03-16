using System.IO;

namespace FramePFX.Core.RBC {
    /// <summary>
    /// The base class for the RBC (REghZy Binary Config... i know right what a sexy acronym)
    /// </summary>
    public abstract class RBCBase {
        public abstract int TypeId { get; }

        protected RBCBase() {

        }
        
        // public static void Read

        public abstract void Load(BinaryReader reader);
        public abstract void Save(BinaryWriter writer);

        // public static RBCBase CreateById(int id) {
        //     switch (id) {
        //         case 1:
        //     }
        // }
    }
}