# MyWPFControls
Just a Collection of WPF controls that i made for code reuse.
Currently there are Four different Controls. The biggest, and most stable one (and the only i will write about atm is)

**Timeline Control**
--------------------------------------------------------------------------------------------------------------------------------
This control works how all other timelines work in any video editor. Here is the list of features that i can currently do/support
1. Adding of new tracks
2. Adding of timeblocks per track via a button
3. Moving of each timeblock, which uses dependacy properties so when ever
you move a block around the time variables are automatically updated
4. Zooming in and out scales the timeline horizontally
5. There is a time line scrubber to show the time in seconds.
6. The scaling is done with my math, no longer a transform. so no more
progating down the stack awkwardly.
7. The ability to snap to other timblocks when holding down the left CTRL
8. The ability to resize time blocks.
9. the abilty to snap on resize, and movement.
10. Sprites, and text of timeblocks are data bound and visually auto update
11. The ability to play, pause, and stop.
12. stopping goes back to where you placed the temp start line.
13. While playing the control keep track of any and all active blocks that
should be shown to the sprites/dialogue boxes.
14. there is also external event support so you can decide how YOU would like
to procede when something is either added or removed from acitve status.

15. The timeline will automatically grow if you drag out of the timeline
bounds.

-----
**Example Using my Tester program**
![Alt text](https://github.com/CodenameColors/MyWPFControls/blob/master/Timeline.png "Optional Title")

**Example with implementation into other projects (Amethyst Engine)**
![Alt text](https://github.com/CodenameColors/MyWPFControls/blob/master/Timeline%20example.png "Optional Title")

To use this control, just download the Timeline.dll file, and reference it in you .NET project!
