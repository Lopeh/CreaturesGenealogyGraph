# CreaturesGenealogyGraph
A genealogy mapper for Creatures 3 / Docking station!

## Install & Use

0. Make sure you have [Python 3]([url](https://www.python.org/downloads/)) and [GraphViz]([url](https://graphviz.org/download/)) installed
1. Move the Quick_Genealogy.cos into your `\Docking Station\Bootstrap` folder or `\Creatures 3\Bootstrap` folder.
2. Copy the `family_tree.py` into a folder of your choice
3. Start the world you want a genealogy map of.
4. Select a norn, and open up the script console with CTRL+SHIFT+C
5. Type in `ject "Quick_Genealogy.cos" 4`
6. The new genealogy file is now generated in the `My Worlds\{WorldName}\Journal\{WorldName}.genealogy`. Copy this file into where you placed the `family_tree.py`
7. In your console of choice, run `python family_tree.py {WorldName}.genealogy`, and a new SVG file will be created and run
