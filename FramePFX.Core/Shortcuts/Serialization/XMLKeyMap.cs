using System.Collections.Generic;
using System.Xml.Serialization;

namespace FramePFX.Core.Shortcuts.Serialization {
    [XmlRoot("KeyMap")]
    public class KeyMap : Group {

    }

    public class Group {
        [XmlAttribute("Name")] public string Name { get; set; }
        [XmlAttribute("Description")] public string Description { get; set; }
        [XmlAttribute("IsGlobal")] public string IsGlobal { get; set; }
        [XmlAttribute("InheritPath")] public string InheritFromParent { get; set; }
        [XmlElement("Group")] public List<Group> InnerGroups { get; set; }
        [XmlElement("Shortcut")] public List<Shortcut> Shortcuts { get; set; }

        [XmlIgnore]
        public bool IsGlobalBool => !string.IsNullOrWhiteSpace(this.IsGlobal) && this.IsGlobal.ToLower().Equals("true");

        [XmlIgnore]
        public bool InheritBool => !string.IsNullOrWhiteSpace(this.InheritFromParent) && this.InheritFromParent.ToLower().Equals("true");
    }

    public class Shortcut {
        [XmlAttribute("Name")] public string Name { get; set; }
        [XmlAttribute("Description")] public string Description { get; set; }
        [XmlAttribute("ActionID")] public string ActionID { get; set; }
        [XmlAttribute("IsGlobal")] public string IsGlobal { get; set; }

        [XmlElement("Keystroke", Type = typeof(Keystroke))]
        [XmlElement("Mousestroke", Type = typeof(Mousestroke))]
        public List<object> Strokes { get; set; }

        [XmlIgnore]
        public bool IsGlobalBool => !string.IsNullOrWhiteSpace(this.IsGlobal) && this.IsGlobal.ToLower().Equals("true");
    }

    public class Keystroke {
        [XmlAttribute("Key")]        public string KeyName { get; set; }
        [XmlAttribute("Keycode")]    public string KeyCode { get; set; }
        [XmlAttribute("Mods")]       public string Mods { get; set; }
        [XmlAttribute("IsRelease")]  public string IsRelease { get; set; }
    }

    public class Mousestroke {
        [XmlAttribute("Button")]      public string Button { get; set; }
        [XmlAttribute("Mods")]        public string Mods { get; set; }
        [XmlAttribute("ClickCount")]  public string ClickCount { get; set; }
        [XmlAttribute("WheelDelta")]  public string WheelDelta { get; set; }
        [XmlAttribute("CustomParam")] public string CustomParamInt { get; set; }
    }
}