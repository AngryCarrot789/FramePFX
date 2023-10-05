namespace FramePFX.WPF.Explorer.Icons {
    public enum CSIDL {
        // anything with a ? uses the default folder icon, and is therefore "dodgy" and not useful
        // anything with [broken; invalid] will cause the GetBitmapSourceForSystemIcon function to fail
        CSIDL_ADMINTOOLS = 0x0030, // ?
        CSIDL_ALTSTARTUP = 0x001d, // ?
        CSIDL_APPDATA = 0x001a, // ?
        CSIDL_BITBUCKET = 0x000a, // trash can
        CSIDL_CDBURN_AREA = 0x003b, // ?
        CSIDL_COMMON_ADMINTOOLS = 0x002f, // ?
        CSIDL_COMMON_ALTSTARTUP = 0x001e, // ?
        CSIDL_COMMON_APPDATA = 0x0023, // ?
        CSIDL_COMMON_DESKTOPDIRECTORY = 0x0019, // ?
        CSIDL_COMMON_DOCUMENTS = 0x002e, // ?
        CSIDL_COMMON_FAVORITES = 0x001f, // favourites (star)
        CSIDL_COMMON_MUSIC = 0x0035, // ?
        CSIDL_COMMON_OEM_LINKS = 0x003a, // [broken; invalid]
        CSIDL_COMMON_PICTURES = 0x0036, // ?
        CSIDL_COMMON_PROGRAMS = 0X0017, // ?
        CSIDL_COMMON_STARTMENU = 0x0016, // ?
        CSIDL_COMMON_STARTUP = 0x0018, // ?
        CSIDL_COMMON_TEMPLATES = 0x002d, // ?
        CSIDL_COMMON_VIDEO = 0x0037, // ?
        CSIDL_COMPUTERSNEARME = 0x003d, // computer screen infront of earth
        CSIDL_CONNECTIONS = 0x0031, // connections
        CSIDL_CONTROLS = 0x0003, // control panel
        CSIDL_COOKIES = 0x0021, // ?
        CSIDL_DESKTOP = 0x0000, // desktop
        CSIDL_DESKTOPDIRECTORY = 0x0010, // same as above icon
        CSIDL_DRIVES = 0x0011, // My PC
        CSIDL_FAVORITES = 0x0006, // favourites (star)
        CSIDL_FLAG_CREATE = 0x8000, // desktop icon
        CSIDL_FLAG_DONT_VERIFY = 0x4000, // desktop icon
        CSIDL_FLAG_MASK = 0xFF00, // desktop icon
        CSIDL_FLAG_NO_ALIAS = 0x1000, // desktop icon
        CSIDL_FLAG_PER_USER_INIT = 0x0800, // desktop icon
        CSIDL_FONTS = 0x0014, // fonts folder
        CSIDL_HISTORY = 0x0022, // ??? history i guess
        CSIDL_INTERNET = 0x0001, // ?
        CSIDL_INTERNET_CACHE = 0x0020, // ?
        CSIDL_LOCAL_APPDATA = 0x001c, // ?
        CSIDL_MYDOCUMENTS = 0x000c, // [broken; invalid]
        CSIDL_MYMUSIC = 0x000d, // music
        CSIDL_MYPICTURES = 0x0027, // pictures
        CSIDL_MYVIDEO = 0x000e, // vids
        CSIDL_NETHOOD = 0x0013, // ?
        CSIDL_NETWORK = 0x0012, // network
        CSIDL_PERSONAL = 0x0005, // my documents
        CSIDL_PRINTERS = 0x0004, // printer stuff with file
        CSIDL_PRINTHOOD = 0x001b, // ?
        CSIDL_PROFILE = 0x0028, // ?
        CSIDL_PROGRAM_FILES = 0x0026, // ?
        CSIDL_PROGRAM_FILES_COMMON = 0x002b, // ?
        CSIDL_PROGRAM_FILES_COMMONX86 = 0x002c, // ?
        CSIDL_PROGRAM_FILESX86 = 0x002a, // ?
        CSIDL_PROGRAMS = 0x0002, // ?
        CSIDL_RECENT = 0x0008, // recent stuff
        CSIDL_RESOURCES = 0x0038, // ?
        CSIDL_RESOURCES_LOCALIZED = 0x0039, // [broken; invalid]
        CSIDL_SENDTO = 0x0009, // ?
        CSIDL_STARTMENU = 0x000b, // ?
        CSIDL_STARTUP = 0x0007, // ?
        CSIDL_SYSTEM = 0x0025, // ?
        CSIDL_SYSTEMX86 = 0x0029, // ?
        CSIDL_TEMPLATES = 0x0015, // ?
        CSIDL_WINDOWS = 0x0024 // ?
    }
}