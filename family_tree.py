"""
Creatures Genealogy Graph
This script is based off Verm's Quick Genealogy CAOS script, and was started with help from ChatGPT and then hacked into working by Mooalot.
Created & tested with Python 3.11.0
"""
import sys
if sys.version_info[0] < 3:
    raise Exception("Python 3 or a more recent version is required.")

import os
import re
from graphviz import Digraph

# Regular expression patterns
status_pattern = re.compile(r"Status:\s+(\d+)")
species_pattern = re.compile(r"Species:\s+(\d+)")
sex_pattern = re.compile(r"Sex:\s+(-?\d+)")
variant_pattern = re.compile(r"Variant:\s+(-?\d+)")
warped_pattern = re.compile(r"Has Warped:\s+(\d+)")

# Dictionary to store creature data
creature_dict = {}

# Create a set to store the genome_monikers of living descendants and their ancestors
living_descendants = set()
living_ancestors = set()

#Change this to show or hide eggs
show_eggs = True

# Chnge to show only ancestors of the living
show_living_only = False

def find_default_genealogy_file():
    """
    Find the default genealogy file in the script folder.
    Returns the path of the first .genealogy file found, or None if no file is found.
    """
    script_folder = os.path.dirname(os.path.abspath(__file__))
    genealogy_files = [file for file in os.listdir(script_folder) if file.endswith('.genealogy')]
    if genealogy_files:
        return os.path.join(script_folder, genealogy_files[0])
    else:
        return None

def split_name_moniker(name_with_moniker):
    parts = name_with_moniker.split()
    if len(parts) > 1:
        name = ' '.join(parts[:-1])
        genome_moniker = parts[-1]
    elif len(parts) == 1:
        name = parts[-1] if '.gen' in parts[-1] else 'Unknown'
        genome_moniker = parts[0]
    else:
        name = None
        genome_moniker = None
    return name, genome_moniker

def create_parent_object(parent_line):
    name, moniker = split_name_moniker(parent_line[1])
    parent = {
        "genome_moniker": moniker,
        "name": name,
        "sex": None
    }
    if parent_line[0] == "Mother":
        parent["sex"] = "female"
    elif parent_line[0] == "Father":
        parent["sex"] = "male"
    elif parent_line[0] == "Unknown":
        parent["sex"] = "unknown"

    return parent

# Function to parse genealogy file
def parse_genealogy(file_path):
    with open(file_path, "r") as file:
        data = file.read()
        
    records = re.split("\n\n", data)
    
    for record in records:
        lines = record.strip().split("\n")
        if len(lines) >= 8:
            name_line = lines[0].split(': ')
            name, genome_moniker = split_name_moniker(name_line[1])
            status_match = re.match(status_pattern, lines[3])

            if status_match:
                # Ignore this creature if the status is 7 or higher
                status = int(status_match.group(1))
                if status >= 7:
                    continue
                if status == 3:
                    living_descendants.add(genome_moniker)

            parentA_line = lines[1].split(': ')
            parentB_line = lines[2].split(': ')
            species_match = re.match(species_pattern, lines[4])
            sex_match = re.match(sex_pattern, lines[5])
            variant_match = re.match(variant_pattern, lines[6])
            warped_match = re.match(warped_pattern, lines[7])

            parentA = create_parent_object(parentA_line)
            parentB = create_parent_object(parentB_line)

            # If the sex of one parent is known, but the other isn't, then we can surmise
            if parentA['genome_moniker'] and parentB['genome_moniker']:
                if (parentA['sex'] == 'unknown') != (parentB['sex'] == 'unknown'):
                    if parentA['sex'] == 'unknown':
                        parentA['sex'] = 'female'
                    if parentB['sex'] == 'unknown':
                        parentB['sex'] = 'female'

            creature_dict[genome_moniker] = {
                "name": name,
                "parents": [],
                "status": None,
                "species": None,
                "sex": None,
                "variant": None,
                "warped": None
            }
            
            if parentA["genome_moniker"]:
                creature_dict[genome_moniker]["parents"].append(parentA)
            
            if parentB["genome_moniker"] and parentB["genome_moniker"] != parentA["genome_moniker"]:
                creature_dict[genome_moniker]["parents"].append(parentB)
            
            if species_match:
                creature_dict[genome_moniker]["species"] = int(species_match.group(1))
            
            if sex_match:
                sex = int(sex_match.group(1))
                if sex == 1:
                    creature_dict[genome_moniker]["sex"] = "male"
                elif sex == 2:
                    creature_dict[genome_moniker]["sex"] = "female"
                elif sex == -1:
                    creature_dict[genome_moniker]["sex"] = "undetermined"
                elif sex == 0:
                    creature_dict[genome_moniker]["sex"] = "non-binary"
            
            if variant_match:
                creature_dict[genome_moniker]["variant"] = int(variant_match.group(1))
            
            if warped_match:
                creature_dict[genome_moniker]["warped"] = int(warped_match.group(1))

# Function to find the ancestors of the living
def dfs_ancestors(genome_moniker, living_ancestors):
    if not genome_moniker in living_ancestors:
        living_ancestors.add(genome_moniker)
    
    if genome_moniker not in creature_dict:
        return
    
    creature = creature_dict[genome_moniker]
    for parent in creature["parents"]:
        if parent["name"] and parent["genome_moniker"] not in living_ancestors:
            dfs_ancestors(parent["genome_moniker"], living_ancestors)

# Remove ancestors unrelated to the living
def remove_nonliving_ancestors():
    living_creature_dict = {}
    for genome_moniker, creature in creature_dict.items():
        if genome_moniker in living_descendants or genome_moniker in living_ancestors:
            living_creature_dict[genome_moniker] = creature_dict[genome_moniker]
    return living_creature_dict

# Creature node styling rules
def creature_node_style(genome_moniker, creature):
    node_options = {'color':'lightgrey', 'shape':'rect', 'fontcolor':'grey', 'fillcolor':'white'}
    egg_options = {'color':'lightgreen','shape':'ellipse'}

    if creature['status'] == 1:
        return egg_options
    
    # Define the shape from living or living ancestry
    if genome_moniker in living_descendants:
        # Living descendant
        node_options['shape'] = 'doublecircle'
        node_options['style'] = 'filled'
        node_options['fillcolor'] = 'lightblue'
        node_options['fontcolor'] = 'black'
    elif genome_moniker in living_ancestors:
        # Living ancestor
        node_options['shape'] = 'circle'
        node_options['style'] = 'filled'
        node_options['fillcolor'] = 'lightgrey'
        node_options['fontcolor'] = 'black'

    # Modify shape if exported
    if creature['status'] == 4:
        node_options['shape'] = 'octagon'

    # Modify color if warped (see: imported)
    if creature['warped'] ==1:
        node_options['fillcolor'] = node_options['fillcolor'] + ':blue'
        node_options['style'] = 'filled'
        node_options['gradiantangle'] = '90'
        # print(node_options)

    # Define color from sex
    if creature['sex'] == 'male':
        node_options['color'] = 'blue'
    elif creature['sex'] == 'female':
        node_options['color'] = 'deeppink'
    elif creature['sex'] == 'non-binary':
        node_options['color'] = 'pink'
    
    return node_options


# Function to render graph
def render_graph(creature_dict, file_name):
    """
    Render the genealogy graph using Graphviz and save it as an SVG file.
    """
    dot = Digraph(comment='Genealogy Graph')
    dot.format = 'svg'

    for genome_moniker, creature in creature_dict.items():
        node_options_gen = {'color':'yellow', 'style':'filled', 'shape':'rect', 'fillcolor':'yellow'}

        if creature['name'] != 'Unknown' or show_eggs is True:
            if not dot.node(genome_moniker):
                node_style = creature_node_style(genome_moniker, creature)
                dot.node(genome_moniker, creature['name'], **node_style)

            for parent in creature['parents']:
                if not dot.node(parent['genome_moniker']):
                    dot.node(parent['genome_moniker'], parent['name'], color='green', shape='polygon', distortion='0.1')
                if parent['sex'] == 'male':
                    dot.edge(parent['genome_moniker'], genome_moniker, color='blue')
                elif parent['sex'] == 'female':
                    dot.edge(parent['genome_moniker'], genome_moniker, color='deeppink')
                else:
                    dot.edge(parent['genome_moniker'], genome_moniker, color='grey')

                if '.gen' in parent['genome_moniker']:
                    dot.node(parent['genome_moniker'], parent['name'], **node_options_gen)

    dot.render(file_name, cleanup=True, view=True)


def main(file_name):
    # Read and process the genealogy file
    print(f'Reading file {file_name}')
    parse_genealogy(file_name)

    render_file_name = file_name.replace('.genealogy','')

    # Perform a depth-first search starting from the living descendants
    print(f'Finding ancestors for the living {file_name}')
    for genome_moniker in living_descendants:
        dfs_ancestors(genome_moniker, living_ancestors)
    
    # If we're showing living descendants only, remove unrelated
    final_creature_dict = creature_dict
    if show_living_only:
        final_creature_dict = remove_nonliving_ancestors()
        render_file_name += '_living-only'
    
    if show_eggs:
        render_file_name += '_eggs'

    # Render and save the genealogy graph
    print(f'Found {len(final_creature_dict)} creature records to render.')
    render_graph(final_creature_dict, render_file_name)


if __name__ == '__main__':
    import sys

    if len(sys.argv) > 1:
        file_name = sys.argv[1]
    else:
        # Set the default file name to the first .genealogy file in the script folder
        file_name = find_default_genealogy_file()

    if file_name:
        main(file_name)
    else:
        print("No genealogy file found. Please provide a valid genealogy file.")
