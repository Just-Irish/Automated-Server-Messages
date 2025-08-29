# Automated Server Messages

**Note:** *This mod was created almost exclusivly with the help of ChatGPT and Claude Sonnet 4. I have negligable knowledge in coding and could not have made this mod without these LLMs.*

## What Does This Mod Do?

With this mod you will be capable of setting up server-wide messages that everybody can see in the in-game chat window using commands in either the server terminal or the in-game chat window.
You can have as many messages as you want and they will stay persistent after a server shutdown or restart, so you will not have to recreate the message. I wanted to create
this mod due to having my own server set up to automatically restart twice a day, but there was never any sort of notification. Thus, when the time came, everyone was
immediately kicked off the server very abruptly. Since there was no way of knowing, players were always in the middle of something when the time came. So now, with this mod,
I can now set up multiple messages that all players will recieve and hopefully warn them of the impending server restart.

Of course, this can be used for other things as well. Perhaps you run a public server and you want to have a recurring message about your Discord, or maybe you want your players
to be aware of an issue that you are aware of and are intending to fix. It's entirely up to you within the capabilities of the script.

**Do Keep In mind** this mod uses the time of the hosts machine. So if you are renting a server you will need to figure out its time zone and base your messages off of that.

## Commands
~~* /automsgtest~~
* /automsg add [HH:mm] [Type your message here with up to 27 words!]
* /automsg remove [index]
~~* /automsg clear~~
* /automsg list

#### Explaining the Commands
1. "**add**" quite obviously will add a message, but you need the rest of the command for it to do so.
2. **[HH:mm]** references the time you want the message to trigger in a 24 hour format. So if you want your message to trigger at 3 in the afternoon (based on the host machines timezone),
   then you would type [15:00] (excluding the brackets).
3. **[Type your message here with up to 27 words!]**. Pretty self explanatory (exclude the brackets again).
4. "**remove**" specifically deletes a single automated message. You need the rest of the command for it to work.
5. **[index]** references the message you wish to delete in the order that it was made.
6. "**list**" will show every single message you have created in the order that they were created.

#### Examples of the Commands Being Used
* /automsg add 05:00 Server restart in 30 minutes.
  * A message will appear in chat stating that there will be a server restart in 30 minutes at 5:00 am, 12:00 pm, and 5:00 pm.

* /automsg remove 4
  * This will remove the fourth message you created if you happen to have 4 automated messages made. If you have 5, the fifth automated message will now become the fourth since you just deleted
  the other one.

* After creating 4 different messages and then using the command "/automsg list" you can see in the image below that it gives each message a number based on when it was created, not when the message is supposed to trigger.
<img width="486" height="248" alt="Image" src="https://github.com/user-attachments/assets/5ddc24ac-dfa3-4d50-bb7b-edd118d2d55b" />
