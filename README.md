# NightDriverUnity
Davepl, 9/19/2021

See Discussions for questions and comments.
See source code and COPYING.txt for detailed technical and licensing information including versions.

What NightDriverUnity is:

NightDriverUnity is a set of scripts for Unity that you can attach to a standard VideoPlayer object.  You then set the IP address, dimensions, and framerate and when you press play the video in question will be streamed to the NightDriverStrip located at that IP address.

To create a suitable client for sending data, use the NightDriverStrip's SPECTRUM config, with the following changes:

    #define ENABLE_WIFI             1  // Connect to WiFi
    #define INCOMING_WIFI_ENABLED   1   // Accepting incoming color data and commands
    #define WAIT_FOR_WIFI           1   // Hold in setup until we have WiFi - for strips without effects
    #define TIME_BEFORE_LOCAL       2   // How many seconds before the lamp times out and shows local content
    #define ENABLE_WEBSERVER        1   // Turn on the internal webserver
    #define ENABLE_NTP              1   // Set the clock from the web
    #define ENABLE_OTA              0  // Accept over the air flash updates
    #define ENABLE_REMOTE           1   // IR Remote Control
    #define ENABLE_AUDIO            1   // Listen for audio from the microphone and process it

I also recommmend that you set

    #define INCOMING_CORE           0
    #define MAX_BUFFERS            30
    
    
Construct a display consisting of three 16x16 matrixes in series, and connect them to PIN5 of an M5 ESP32 stick.  Set your WiFi credentials in globals.h, and when run, the client should get (and display) an IP address.  Use this IP in Unity on the LEDSignController object.  With that set you should be able to click Play and have PACMAN scroll across your LED display.  Good luck!
