A basic GUI for the AI-Foosball Capstone Project.

run the application in developer mode using:
`npm run start` 
in a command terminal. 

To bring up the developer tools (while the app is running):
`CTRL+SHIFT+I`

To compile, from the command line:
`npx electron-packager .`
The app will be packaged into `./foosball-gui-linux-armv7l`. 

Use `./foosball-gui-linux-armv7l/foosball-gui` to run the compiled app.

# Graphical User Interface
The graphical user interface was built using a number of *web based toolkits* chief among those are: JavaScript, ElectronJS, NodeJS, HTML, and CSS. If you aren't familiar with any of these there are a multitude of guides that teach you everything you could possibly need to know about it. 

Now before you say, "Why in the world did he build the desktop app using JavaScript?!" I did it because I am familiar with the language and it was really easy to make using Electron. 

If you look through `preload.js` (./src/preload.js) on line 61 is where the script polls the server for the data. Looking through this section tells you *exactly* what elements of the JSON object the GUI is looking for to update the player_score and robot_score fields in the app. 

## How I built It
The app is built using really basic HTML and JavaScript. ElectronJS is only used to run the app in a Desktop app. The caveat is that ElectronJS alters the running scope of backend and frontend processes.

This causes a problem when you want to write code for the frontend that relies on NodeJS modules. The change in scope that Electron introduces prevents Node modules from being used in the frontend application. This blocks the usage of the `net` module, which allows us to poll the server for the game state.

However, ElectronJS provides a way to "preload" JavaScript into the frontend application. `index.js` is the entrance for the Electron app which creates and loads the window, it also allows for these "preloaded" scripts to be added to the app. This means that we can use Node modules in frontend code, resolving the challanges with process scope. 

> I'm fairly confident there is a better way to do this but the approach requires a greater depth of knowledge than I possess. After spending hours upon hours reading the electronJS documentation, the approach I used was the one that *easily* fit our purposes. 

## Links
Here are couple links that I found useful when building the app:

- Resolving NPM EACCESS Error (this was useful in solving a linux permissions error I was getting)
	- <https://docs.npmjs.com/resolving-eacces-permissions-errors-when-installing-packages-globally>
- Auto-start NPM (sets the app to run at boot)
	- <https://www.npmjs.com/package/auto-launch>
- Electron Packager (packages the app, needed for startup-at-boot)
	- <https://github.com/electron/electron-packager>
