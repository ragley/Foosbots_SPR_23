# AI-Foosball

## Links to Other READMEs

- [DHCP README](pi/DHCP.md)
- [GUI README](pi/foosball-gui/README.md)
- [Data Server README](pi/MainServer/README.md)
- [Jetson User/Pass](jetson/README.md)
- [Vision README](jetson/image_processing/README)
- [FSM README](jetson/FiniteStateMachine/README.md)
- [ESP32 Config README](pi/ESP32/README.md)
- [Controller.ino README](ESP32/Motor_Controller/Controller/README.md)
- [UNO README](UNO/README.md)
- [CAN Messages README](ESP32/Docs/CAN_messages.md)

# Production vs Development Code

All production code is set as bootup scripts. These scripts are stored in the `/bin` directory and in general should never be directly edited (rather overwritten from the development repo).

To run and test development code, the respective production code has to be stopped. For example if a new ball*tracking algorithm is being written then the \_ball_tracking.service* must be stopped before the new code can be ran. To do this you would run the following command:
`sudo systemctl stop ball_tracking.service`

To determine what script is running what code you'll have to go to `/etc/systemd/system/` and look for the .service that corresponds to whatever program you are trying to run.

After your development code is ready to be placed into production follow the below commands to either create a new startup script or update an existing one.

## Running Python Scripts at Startup

Follow the instructions here: <https://unix.stackexchange.com/questions/634410/start-python-script-at-startup> it's fairly self-explainatory.

For our startup scripts we copied the python codes we wanted to run at start into the `/bin` folder. This allows the script to be run with superuser permissions. For reference, the command to copy to the `/bin` directory is:
`sudo cp ./"script name" /bin/"script name"`

> Note that if you are editing an already existing file (i.e. ball_tracking.py) there likely already exists a service for it. All you would need to do would be to recopy the code to the `/bin` directory (as seen above).

This would mean the .service file (in `/etc/systemd/system/myscript.service`) would like like:

```
[Unit]
Description=My Script

[Service]
ExecStart=/usr/bin/python3 /bin/"your script here".py

[Install]
WantedBy=multi-user.target
```

On thing of note however, this will not work for any _graphical_ application (i.e. a gui). The script will also crash if you try to open a window (such as to display the camera output). It handles print statements fine, athough you won't see them unless you run `sudo systemctl status "your service here".service`.

The alternative is to use a `crontab` to set things at startup, but you'll have to figure that out yourself.
