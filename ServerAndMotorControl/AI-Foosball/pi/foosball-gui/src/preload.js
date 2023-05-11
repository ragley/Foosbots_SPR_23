// preload.js
const net = require("net");

const HOST = "192.168.0.1";
const PORT = 5000;

// establish connection to the server
const client = new net.Socket();
try{
  client.connect({port: PORT, host: HOST});
}
catch (error){
  console.log("Cannot find server...");
}


let userAction = {
  "action": "POST"
};

let serverRequest = {
  "action": "DUMP"
};



// All of the Node.js APIs are available in the preload process.
// It has the same sandbox as a Chrome extension.
window.addEventListener("load", () => {
  let startGameBtn = document.getElementById("startGameBtn");
  let pauseGameBtn = document.getElementById("pauseGameBtn");
  let isGamePaused = false;
  userAction["pause"] = 0;
  client.write("empt"+JSON.stringify(userAction));
  let endGameBtn = document.getElementById("endGameBtn");
  
  let osuLogo = document.getElementById("osuLogo");
  let devModal = document.getElementById("devModal");
  let devModalCloseBtn = document.getElementById("devModalCloseBtn");
  
  let difficultyModal = document.getElementById("difficultyModal");
  let difficultyModalCloseBtn = document.getElementById("difficultyModalCloseBtn");
  let easyBtn = document.getElementById("easyBtn");
  let mediumBtn = document.getElementById("mediumBtn");
  let hardBtn = document.getElementById("hardBtn");
  
  
  // Event Listeners ===================================================
  startGameBtn.addEventListener("click", () => {
    difficultyModal.style.display = "block";
  });
  pauseGameBtn.addEventListener("click", () => {
    if(!isGamePaused){
      userAction["pause"] = 1;
      client.write("empt"+JSON.stringify(userAction));
      pauseGameBtn.innerHTML = "Resume Game";
      isGamePaused = !isGamePaused;
    }
    else {
      userAction["pause"] = 0;
      client.write("empt"+JSON.stringify(userAction));
      pauseGameBtn.innerHTML = "Pause Game";
      isGamePaused = !isGamePaused;
    }
  });
  endGameBtn.addEventListener("click", () => {
    userAction["game_flag"] = 0;
    client.write("empt"+JSON.stringify(userAction));
  });
  
  // difficulty modal
  
  easyBtn.addEventListener("click", () => {
    difficultyModal.style.display = "none";
    userAction["game_flag"] = 1;
    userAction["difficulty_flag"] = 0;
    console.log(userAction);
    client.write("empt"+JSON.stringify(userAction));
  });
  mediumBtn.addEventListener("click", () => {
    difficultyModal.style.display = "none";
    userAction["game_flag"] = 1;
    userAction["difficulty_flag"] = 1;
    console.log(userAction);
    client.write("empt"+JSON.stringify(userAction));
  });
  hardBtn.addEventListener("click", () => {
    difficultyModal.style.display = "none";
    userAction["game_flag"] = 1;
    userAction["difficulty_flag"] = 2;
    console.log(userAction);
    client.write("empt"+JSON.stringify(userAction));
  });
  
  // dev modal
  osuLogo.addEventListener("click", () => {
    devModal.style.display = "block";
  });
  
  devModalCloseBtn.addEventListener("click", () => {
    devModal.style.display = "none";
  });
  
});



// periodically poll the server for data
setInterval(() => {
  let devModalContentText = document.getElementById("devModalContentText");
  let playerScore = document.getElementById("playerScore");
  let robotScore = document.getElementById("robotScore");
  // console.log(JSON.stringify(serverRequest));
  if (client.write("empt"+JSON.stringify(serverRequest))){
    client.on('data', (data) => {
      let json_data = JSON.parse(data);
      // console.log(JSON.stringify(json_data, null, 4)); 
      devModalContentText.innerHTML = JSON.stringify(json_data, undefined, 4);
      if("player_score" in json_data){
        playerScore.innerHTML = json_data["player_score"];
      }
      if("robot_score" in json_data){
        robotScore.innerHTML = json_data["robot_score"];
      }
    });
  }
  else {
    console.log("Error sending message...");
    client.connect({port: PORT, host: HOST});
  }
}, 800);
