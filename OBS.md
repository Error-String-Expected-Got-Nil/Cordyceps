# OBS Integration
Cordyceps has a companion OBS plugin called Cordyceps-stalk, the Github page for which can be found here: https://github.com/Error-String-Expected-Got-Nil/Cordyceps-stalk

If this plugin is installed, Cordyceps can communicate with it and allow you to record real-time footage while you play in slow motion. Instructions for installing it are included with the plugin's releases.

## Compatibility
Cordyceps-stalk has only been tested for Windows. I do not know if it will work for Linux and can't really test it, but I suspect it won't work, and I cannot offer any troubleshooting for it.

## Setup
Before setting up Cordyceps, you'll need to make sure some things are enabled. Open OBS, look at the top bar, and select the **Tools**, then **WebSocket Server Settings**. This will open a panel with some details. At the top, there should be a checkbox labeled **Enable WebSocket Server**. Turn it on. This allows external applications to connect to OBS, and Cordyceps uses it to communicate with OBS.

Next, you'll need to copy the server password. It's hidden but editable by default, either change it to something you'll remember, or click the **Show Connect Info** button, and copy it from there. You could also leave it blank. Click **Apply** to save the settings, then you may exit the window.

Assuming you have already installed Cordyceps-stalk, that should be everything set up for OBS.

On Cordyceps' end, there are a couple more things to do. You'll need to naviagate to Rain World's ModConfigs directory. On Windows, you should be able to get to it by pasting the following path into file explorer: `%appdata%/../LocalLow/Videocult/Rain World/ModConfigs/`. From there, if it does not already exist, create a folder named `Cordyceps`. Inside this folder, you will need two config JSON files: `websocket_config.json` and `encoder_config.json`. Both will be automatically created with some default settings if they don't exist when Cordyceps needs them, otherwise, you can paste the following respective code blocks into each and modify the settings as you wish:

`websocket_config.json`
```
{
    "password": "",
    "port": 4455
}
```

Password is the WebSocket Server password you copied earlier. Port is also listed in the same screen, and is `4455` by default, you shouldn't need to change it.

`encoder_config.json`
```
{
    "dirpath": "C:/cordyceps/",
    "keyframe_interval": 120,
    "crf": 23.0,
    "preset": "veryfast"
}
```

These are settings for the encoder/output used by Cordyceps-stalk to make the video. Dirpath is the only really relevant one, it's the path to the directory you want recordings to be saved to. **NOTE:** This is *not* the name of the file that is saved, it is the directory you want the files saved to. The default `C:/cordyceps/` path means recordings are saved to a folder named `cordyceps` in the root `C:` directory. For technical reasons, this path must end in a `/` or `\`, and will default back to `C:/cordyceps/` if it doesn't.

The other settings are only relevant to the video encoder, and are some configuration options for the libx264 H264 encoder used by Cordyceps-stalk. If you don't know what that means, just leave them on the default values shown here.

Note that the `ModConfigs` directory is included in Steam cloud saves, which might be inconvenient in this case. If you can't get the files to save properly, disable it temporarily or make your changes while Rain World is open to circumvent it.

In Rain World, you will also need to set the Recording FPS in Cordyceps' remix settings. Make sure it's the same as what OBS is set to, or recordings made will not be at the right speed!

## Usage
From here, things should be much easier. Cordyceps will automatically connect with OBS when Rain World launches, though this will fail if OBS isn't running or Cordyceps-stalk wasn't installed (and in the later case, you will need to restart both Rain World and OBS in order to connect properly after installing Cordyceps-stalk). If OBS wasn't running, there's a keybind to try connecting (default key: \[Y\]).

There are two other keybinds to start recording and stop recording (default keys: \[R\] and \[T\] respectively). Cordyceps will automatically handle things from here. While in the game simulation, details about OBS and recording status will be shown on the info panel, and if there's an unexpected disconnection, Cordyceps will automatically pause the game while trying to reconnect (or until it times out).

Most details about connection status will be printed to Rain World's `consoleLog.txt`, you can check that if you need to see if something is going on.

## Limitations and Known Issues
At the time of writing, this feature has *not* been extensively tested, and while I'm fairly sure it should be good enough to use, I can't guarantee it will be stable over long periods. The system isn't exactly robust, and you can get the connection status into weird states that it can't recover from. If this happens, restarting both OBS and Rain World should always fix it, and I'm fairly certain any in-progress recordings should still save correctly even if stopped by closing OBS.

You may also notice a large number of reconnections in the log, this is normal behavior. For reasons I'm not sure of, the connection just seems to be somewhat unstable, and has a habit of disconnecting randomly- most often during loading screens. I don't know how to fix this, so hopefully the reconnection mechanism is good enough to solve it. There seem to be fewer issues when recording is started while already in the game simulation, though I'm not sure on that.

Also, Cordyceps-stalk can't capture sound. I don't think there's a practical way to correct this, unfortunately. Any recordings made with Cordyceps will simply have to be silent.
