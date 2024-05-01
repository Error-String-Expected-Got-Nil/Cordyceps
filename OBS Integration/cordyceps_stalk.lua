---@diagnostic disable: lowercase-global
obs = obslua

function test_callback()
    print("Test!")
end

function script_load()
    test_hotkey_ID = obs.obs_hotkey_register_frontend("cordyceps_stalk.test", "Cordyceps Stalk Test Hotkey", test_callback)
end

function script_unload()
    obs.obs_hotkey_unregister(test_callback)
end

function script_description()
    return [[
Companion script for the Rain World mod Cordyceps. Responds to hotkey events sent by the mod to pause/unpause OBS and ensure only the desired frames are recorded.]]
end