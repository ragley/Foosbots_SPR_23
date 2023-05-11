Here's where things get a little tricky...

# Background
To allow communication between the Jetson and the Pi, we created a "closed-loop" network. Meaning that we could connect the Jetson and the Pi with an Ethernet cable and use TCP Sockets/IP to communicate data between the two devices. 

Before I get into the *how*, I'll first explain the *why*. 

Our original intention was to use UART to serially communicate data between the Jetson and the Pi. Easy right? Well... not quite. The Jetson has a multitude of UART ports across its various GPIO pins, however, I could not figure out how to get any of them to work. No amount of searching on the internet provided me with any answers... at least not ones I could understand. It didn't help that the Jetson comes packed with a custom Ubuntu OS, pre-configured with all sorts of neat things that are utterly useless to our project and served to do nothing more than hinder me. 

Thus, I decided to use an Ethernet Cable to communication between the two devices. This, however, it not quite as simple as I make it out to be. In order to get this to work I had to setup a DHCP server on the Pi.

## DHCP
If you're not familiar with DHCP, I would highly suggest reading the information on this [link](https://docs.microsoft.com/en-us/windows-server/troubleshoot/dynamic-host-configuration-protocol-basics). 

I converted the `eth0` port of the Pi into a DHCP server. Meaning it'll only respond to DHCP requests on that interface. 

Essentially, a DHCP server allows the Pi to respond to a clients request for an IP address. When the Jetson (or any device for that matter) connects to the Pi's ethernet port, the Pi assigns an IP address to the Jetson. 

The Pi has a static IP address assigned to its `eth0` interface, meaning that whatever device is connected to it can ping the Pi at `192.168.0.1` and always find the Pi. This is what allowed us to use a TCP Client to send location data from the Jetson to the Pi. 

The specific steps I used to make the DHCP server can be found here: <https://www.technicallywizardry.com/building-your-own-router-raspberry-pi/>

## Connecting to the Internet
The downside to this is that *you cannot use the `eth0` port to connect to the internet*, which can be troublesome if a wireless access point (which I will now lovingly refer to as "WAP"). Although if you have a hotspot on your phone that would also work.

However, if you want to connect to the internet, there are a couple steps you'll have to follow (you're essentially turning off the DHCP server and removing the static IP on the `eth0` interface).

- - -

First you'll want to turn off the DHCP server by running:

`sudo systemctl disable isc-dhcp-server.service`

Then you want to navigate (in a terminal) to: `/etc/network/interface.d` (for the uninitiated run `cd /etc/network/interfaces.d`). 

Open the `eth0` file (`sudo nano eth0`) and flip the commented lines. This file should look something like this:

After edit:
```
# for static IP and DHCP
#allow hotplug eth0
#iface eth0 inet static
#	address 192.168.0.1
#	netmask 255.255.255.0
#	gateway 192.168.0.1

# for general eth0 use
auto eth0
iface eth0 inet manual
```

Then reboot the Pi: `sudo reboot`.

Tada! That should do it!

- - -

To turn it back on you'd do the opposite:

`sudo systemctl enable isc-dhcp-server.service`

Flip the commented lines in `/etc/network/interface.d/eth0` to look like this:
```
# for static IP and DHCP
allow hotplug eth0
iface eth0 inet static
	address 192.168.0.1
	netmask 255.255.255.0
	gateway 192.168.0.1

# for general eth0 use
#auto eth0
#iface eth0 inet manual
```

Then restart the pi, `sudo reboot`.

Now the DHCP server is back online. 

## Troubleshooting 
If for some reason it's not working you can run `sudo systemctl status isc-dhcp-server.service` and see if it had problems at boot. Otherwise... google is your best friend.

