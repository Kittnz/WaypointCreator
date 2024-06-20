# ![logo](images/Fire%20Elemental.png) Waypoint Creator for vmangos

This fork of Waypoint Creator supports vmangos and has been improved to handle large sniffs better.

Used to create paths for NPCs on WoW private servers.
Note: This software requires a parsed packet file to work.

## Login

When you start the program you will see the following window. If you have access to a database, you can connect to it so the program can provide the creatures' names for the entries you have sniffed. Just input the data required to connect to your database and click ok. If "Save values" is checked, it will save your data so next time you use the software you will just need to click ok. Connecting to a database is used to display creatures' names and is recommended but not required, and if you do not wish to, you can just click cancel.

![login_screen](images/login_screen.png)

## Main Window

After you log in to your database or click cancel, you will be presented with the main window. Click the "Import Sniff" button and you will see an Open file window.

![main_window](images/main_window.png)

Select a parsed pkt file on your computer to load into the program and click the Open button. The program will then collect all the movement packets from the sniff file.

![open_file](images/open_file.png)

You will now see that a file has been loaded into the program, ready to examine the movement packets.

![loaded_file](images/loaded_file.png)

You can now search through the movement packets by all, leaving the entry textbox blank, or you can input a creature entry to narrow the search as shown below.

![search_all](images/search_all.png)
![search_entry](images/search_entry.png)

You will see a list of GUIDs the program has collected movement packets for, if any. You can then select a GUID by clicking on it, and you will be shown its collected movement packets displayed in a list and on a grid.

This group of movement packets shown here is obviously random movement and not a path, but it is still useful. First, note that "Gray Bear" is shown because I logged into my database. If I did not log in, this would show "Entry XXXXX database not connected." Also, if you are connected to a database and the sniff contains a creature not found in that database, you will see "Entry XXXXX not in database."

If you look in the top right of the window, you will see Wander Range. This is a calculation of the move radius based on the distance between the two farthest collected move points. This will be more accurate with more points collected. Note that if you see a spike where the creature moved into combat, you may wish to delete those points for a more accurate calculation. I will get into cutting and pasting when I discuss paths. Here I would set the wander distance for this creature to 10.

![make_go_xyz](images/make_go_xyz.png)

If you wish to locate this NPC in game, you can right-click on a point and then click "Make .go xyz". In this case, it will copy ".go xyz -320.78665 -787.35364 56.93638" to your clipboard so you can paste it in-game to take you to the location. Note: Make sure you are on the right map before doing this, or you may find yourself falling.

## Pathing "The main reason we are here"

Below we see a sniff that appears to be a path. It is important to note that this software does not detect paths but will show you movement packets on a graph, and it is up to you to find the path in the points. Here we can see that the movement is repeating, thus a path, but some points appear more than once and need to be removed.

![path_circular_sniff](images/path_circular_sniff.png)

Looking at the path, you can see that it goes from point 1 to point 10 and then repeats, so we should remove all the points above 10 by selecting them, then right-clicking on the points and clicking cut.

![path_circular_cut](images/path_circular_cut.png)

After cutting the duplicate points, we are left with the path shown below.

![path_circular_fixed](images/path_circular_fixed.png)

You can then right-click on the waypoints again and click "Create SQL" to output your path to the textbox on the SQL Output Tab. Click on the SQL Output tab, and you will see your SQL for the path.

It's very important to note that the creature_addon INSERT line is generic. The values given will be good for most creatures but may be wrong for some. If the NPC has a mount or auras, you will need to update it before inserting it into the DB, or you will erase them. Also, you will need to make sure bytes2 is correct.

![path_circular_output](images/path_circular_output.png)