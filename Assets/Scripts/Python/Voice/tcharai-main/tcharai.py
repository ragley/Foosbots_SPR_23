from playwright.sync_api import Playwright, sync_playwright, expect
from elevenlabslib import *
from elevenlabslib.helpers import *
import curses
import requests
import sys
import time
import datetime
import io
import json
import os

def run(stdscr):
    if len(sys.argv) > 1:
        chara_id = sys.argv[1]
    else:
        chara_id = "mQFd2rrRf0WwZXKdhToZN3v_-qDq3DvPOhrqscpdND0"
    browser = playwright.firefox.launch(headless=True)
    context = browser.new_context()
    context.clear_cookies()
    page = context.new_page()
   
    page.goto('https://beta.character.ai/chat?char='+chara_id)
    page.get_by_role("button", name="Accept").click()
    
    apiKey = ""
    user = ElevenLabsUser(apiKey)
    voice_name = "hk47"
    voice = user.get_voices_by_name(voice_name)[0]

    filename_txt = voice_name + ".txt"
    file_object = open(filename_txt, 'a')
    sample_num = 1

    while True:
        stdscr.refresh()
        stdscr.addstr("> ")
        stdscr.refresh()
        curses.echo()
        now = datetime.datetime.now()
        time_str = "[{:%H:%M}] ".format(now)
        message = stdscr.getstr().decode()
        page.get_by_placeholder("Type a message").fill(message)
        page.get_by_placeholder("Type a message").press("Enter")
        chara = page.query_selector('div.chattitle.p-0.pe-1.m-0')
        chara_name = chara.inner_text()
        page.wait_for_selector('.swiper-button-next').is_visible()
        div = page.query_selector('div.msg.char-msg')
       
        output_text = div.inner_text()
        filename_wav = str(sample_num) + ".wav"
        file_object.write(output_text + '\n \n')
        #voice.generate_and_stream_audio(output_text, stability=0.22, similarity_boost=0.9, streamInBackground=True)
        
        mp3Data = voice.generate_audio_bytes(output_text)
        save_bytes_to_path(filename_wav, mp3Data)
        play_audio_bytes(open(filename_wav,"rb").read(),True)
        stdscr.addstr(time_str + chara_name + ' ✉\n' + output_text + '\n \n')
        
        stdscr.refresh()
        if stdscr.getch() == 27:
            break

        sample_num += 1
    
    file_object.close()
    context.close()
    browser.close()

if __name__ == '__main__':
    with sync_playwright() as playwright:
        curses.wrapper(run)


# from playwright.sync_api import Playwright, sync_playwright, expect
# import curses
# import sys
# import time
# import datetime

# def run(stdscr):
#     if len(sys.argv) > 1:
#         chara_id = sys.argv[1]
#     else:
#         chara_id = "zb7I4U9OYfewmEgOWLBHScefPeELkm1J-_GZDjHLY1M"
#     browser = playwright.firefox.launch(headless=True)
#     context = browser.new_context()
#     page = context.new_page()
#     page.goto('https://beta.character.ai/chat?char='+chara_id)
#     page.get_by_role("button", name="Accept").click()

#     while True:
#         stdscr.refresh()
#         stdscr.addstr("> ")
#         stdscr.refresh()
#         curses.echo()
#         now = datetime.datetime.now()
#         time_str = "[{:%H:%M}]".format(now)
#         message = stdscr.getstr().decode()
#         page.get_by_placeholder("Type a message").fill(message)
#         page.get_by_placeholder("Type a message").press("Enter")
#         chara = page.query_selector('div.chattitle.p-0.pe-1.m-0')
#         chara_name = chara.inner_text()
#         page.wait_for_selector('.swiper-button-next').is_visible()
#         div = page.query_selector('div.msg.char-msg')
#         output_text = div.inner_text()
#         stdscr.addstr(time_str+ chara_name + ' ✉\n' + output_text + '\n \n')
#         stdscr.refresh()
#         if stdscr.getch() == 27:
#             break

#     context.close()
#     browser.close()

# if __name__ == '__main__':
#     with sync_playwright() as playwright:
#         curses.wrapper(run)
