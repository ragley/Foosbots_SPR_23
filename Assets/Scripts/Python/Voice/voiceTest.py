#!/Foosball Sim/env/pyenv377/Scripts/
from elevenlabslib import *
import time

apiKey = "a86c4cc123e533706ec3686e24154f6a"
user = ElevenLabsUser(apiKey)

voice = user.get_voices_by_name("Dagoth Ur, of the 6th House and Tribe Unmourned")[0]
voice2 = user.get_voices_by_name("freeman")[0]
voice.generate_and_play_audio("Filthy Outlander", playInBackground=False)
voice2.generate_and_play_audio("Come here Daddy", playInBackground=False)


time.sleep(10)