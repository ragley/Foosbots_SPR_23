const { app, BrowserWindow, ipcMain } = require("electron");
const AutoLaunch = require('auto-launch');
const path = require("path");

// create and display window ===========================================
// load the respective html
const createWindow = () => {
  mainWindow = new BrowserWindow({
    fullscreen: true,
    show: false,
    webPreferences: {
      preload: path.join(__dirname, "./src/preload.js"),
    },
  });
  // mainWindow.setMenu(null);
  mainWindow.loadFile("./src/index.html");
  
  mainWindow.once("ready-to-show", () => {
    mainWindow.show();
    setTimeout(() => {
        mainWindow.focus();
    }, 200);
  });
};

app.whenReady().then(() => {
  createWindow();

  app.on("activate", () => {
    if (BrowserWindow.getAllWindows().length === 0) createWindow();
  });
  app.on("ready", () => {
    // Set autolaunch ==================================================
    let autoLauncher = new AutoLaunch({
      name: "foosball-gui",
      path: "./foosball-gui-linux-armv7l/foosball-gui"
    });

    autoLauncher.isEnabled().then((isEnabled) => {
      if(isEnabled) return;
      console.log("Autolauncher enabled :)");
      autoLauncher.enable();
    }).catch((err) => {
      throw err;
    });
    // -----------------------------------------------------------------
  });
});



app.on("window-all-closed", () => {
  if (process.platform !== "darwin") app.quit();
});
// ---------------------------------------------------------------------

