# Space Engineers Ice Storage Script
A script for the game Space Engineers that manages ice storage automatically.

# How To Use
- Add inventories (small cargo container, large cargo container, e.g.) and make sure the name of the inventory (or inventories) contains at least "ICE STORAGE"
 - If the inventory is added after the script has been initialized, you will need to press "recompile" in the programmable block interface.
- Start the script

# Features
- Will not take ice from O2/H2 generators
- Will automatically manage collector power status(es) given the "ICE STORAGE" inventories available capacity.

# Important notes
This script does NOT take into account grid membership. Any and all connected grids will be treated as one (drills, ships, e.g.)
