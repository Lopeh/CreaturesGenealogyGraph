# CreaturesGenealogyGraph
A genealogy mapper for Creatures 3 / Docking station! 

## What is this?

This is a set of in-game and OS-terminal scripts to visualize the ancestry of your creatures like so!
![image](https://github.com/Lopeh/CreaturesGenealogyGraph/assets/13429544/1994e536-6caa-4439-9cf9-3c327e20a480)

Things get much more interesting for longer-running worlds, especially those that export/import creatures or use the warp regularly;
![image](https://github.com/Lopeh/CreaturesGenealogyGraph/assets/13429544/540cfefd-5f49-4fdf-b251-6fc2a02ad472)

## So many colors and shapes!
* Blue, double-circle shapes with names are creatures that are currently alive (and active) in your world!
* Gray, regular-circle shapes with names are the ancestors of those living creatures
* White, regular-circles: Creatures who have passed, but do not have any living descendants
* White, regular-circles (with gray font): Creatures who have passed, but never had *any* offspring
* Polygons: creatures who are referenced only by their 
* If a creature's name is colored green on the left, that means it warped in from another world

![image](https://github.com/Lopeh/CreaturesGenealogyGraph/assets/13429544/60cbd6f8-d6ee-4f2a-a788-6760d5414312)
_Poaditucna here stumbled upon this world, and despite her passing, her lineage is still going!_


![image](https://github.com/Lopeh/CreaturesGenealogyGraph/assets/13429544/438e7e63-254d-487b-9f37-72cb99e01c99)
_Kialryrfoy here is thriving in this new world, even finding love and having a kid!_


* If a creature's name is colored peach on the right, that means it was exported or warped out from the world.

![image](https://github.com/Lopeh/CreaturesGenealogyGraph/assets/13429544/5afee3df-3cb1-4bf4-94ad-375f85e342c1)
_Xoasbacyzu, on the other hand, took a look around, found all this quite silly and strange, and left for new horizons back through the warp_

![image](https://github.com/Lopeh/CreaturesGenealogyGraph/assets/13429544/936deb01-1092-4a4d-a7c5-5f66e8755170)
_Yaasutbof, an engineered norn of the great dave.gen, found a deep sense of wanderlust, and went to explore the Warp, leaving little behind_

## Install & Use

0. Make sure you have [Python 3]([url](https://www.python.org/downloads/)) and [GraphViz]([url](https://graphviz.org/download/)) installed
1. Move the `Quick_Genealogy.cos` file into your `\Docking Station\Bootstrap` folder or `\Creatures 3\Bootstrap` folder.
   a. If you want the script to auto-run every two hours, add the `Quick_GenealogyAgent.cos` file to 
3. Copy the `family_tree.py` into a folder of your choice
4. Start the world you want a genealogy map of.
5. Select a norn, and open up the script console with CTRL+SHIFT+C
6. Type in `ject "Quick_Genealogy.cos" 4`
7. The new genealogy file is now generated in the `My Worlds\{WorldName}\Journal\{WorldName}.genealogy`. Copy this file into where you placed the `family_tree.py` file.
8. In your console of choice, run `python family_tree.py {WorldName}.genealogy`, and a new SVG file will be created and run
   a. In the case you have multiple python installations in Windows, you may need to use `py -3.11 family_tree.py {WorldName}.genealogy`

## The files

### Quick_Genealogy.cos
   This is the meat of this project; a file that iterates through creature life history, and outputs into a file that can be read and visually mapped. The 

### Quick_GenealogyAgent.cos
   An agent that can be injected into your world to auto-run the same Quick Genealogy exporter, but with a _{datetime} suffix in the file name. This is done to preserve data otherwise purged by the game after a number of generations. All credit to this and the Quick_Genealogy code goes to Verm on the Norn Nebula discord

### family_tree.py
   This converts the outpute .genealogy files to .dot and .SVG using [GraphViz]([url](https://graphviz.org/)https://graphviz.org/). For now, this is just the MVP/POC of this project, but will likely be maintained/tweaked periodically.
