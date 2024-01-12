namespace FramePFX {
    public delegate void GenericEventHandler<in T>(T sender);
    public delegate void GenericEventHandler<in T, in A>(T sender, A a);
    public delegate void GenericEventHandler<in T, in A, in B>(T sender, A a, B b);
    public delegate void GenericEventHandler<in T, in A, in B, in C>(T sender, A a, B b, C c);
}