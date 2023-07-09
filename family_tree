"""
Creatures Genealogy Graph

This script is based off Verm's Quick Genealogy CAOS script, and was started with help from ChatGPT and then hacked into working by Mooalot.
Created & tested with Python 3.11.0
"""

import sys
import os
import graphviz

def split_name_moniker(name_with_moniker):
    parts = name_with_moniker.split()
    if len(parts) > 1:
        name = ' '.join(parts[:-1])
        genome_moniker = parts[-1]
    elif len(parts) == 1:
        name = parts[-1] if '.gen' in parts[-1] else 'Unknown'
        genome_moniker = parts[0]
    else:
        name = 'Unknown'
        genome_moniker = 'unknown'
    return name, genome_moniker


def get_generation_level(generation, total_generations):
    if total_generations <= 10000:
        return int(generation)
    return int(generation * (total_generations / 10000))


def dfs_ancestors(genome_moniker, living_ancestors):
    if not genome_moniker in living_ancestors:
        living_ancestors.add(genome_moniker)
    
    if genome_moniker not in person_dict:
        return
    
    person = person_dict[genome_moniker]
    mother_moniker = person['mother_moniker']
    father_moniker = person['father_moniker']
    
    if mother_moniker != 'unknown' and mother_moniker not in living_ancestors:
        dfs_ancestors(mother_moniker, living_ancestors)
    if father_moniker != 'unknown' and father_moniker not in living_ancestors:
        dfs_ancestors(father_moniker, living_ancestors)


# If no input file name is provided, use 'family_tree.txt' as the default
input_file = 'family_tree.txt'
show_eggs = False
# Check if an input file name is provided as a command-line argument
if len(sys.argv) > 1:
    input_file = sys.argv[1]
    if len(sys.argv) > 2:
        show_eggs = sys.argv[2]

# Check if the input file exists
if not os.path.isfile(input_file):
    print(f"Input file '{input_file}' does not exist.")
    sys.exit(1)

# Generate the output file name based on the input file name
output_file = "AI-Graph-" + os.path.splitext(input_file)[0]

# Open the family tree file
with open(input_file, 'r') as file:
    data = file.read()

# Split the data into individual records
records = data.split('\n\n')

# Create a dictionary to store person information based on genome moniker
person_dict = {}
mothers_set = set()
fathers_set = set()

# Create a set to store the genome_monikers of living descendants and their ancestors
living_descendants = set()
living_ancestors = set()

# Iterate over each record and extract the information
for record in records:
    lines = record.split('\n')
    name_line = lines[0].split(': ')
    mother_line = lines[1].split(': ')
    father_line = lines[2].split(': ')

    name, genome_moniker = split_name_moniker(name_line[1])    
    mother_name, mother_moniker = split_name_moniker(mother_line[1]) if len(mother_line) > 1 else ('', 'unknown')
    father_name, father_moniker = split_name_moniker(father_line[1]) if len(father_line) > 1 else ('', 'unknown')
    generation = int(genome_moniker.split('-')[0])

    # Add the person to the dictionary
    # if name != 'Unknown':
    person_dict[genome_moniker] = {
        'name': name,
        'mother_name': mother_name,
        'mother_moniker': mother_moniker,
        'father_name': father_name,
        'father_moniker': father_moniker,
        'generation': generation
    }
    
    """
    # Placeholder logic if the CAOS script gets updated with an 'is alive' flag
    if is_alive:
        living_descendants.add(genome_moniker)
    """
    
    # Add the mother & father to their respective dicts
    if mother_moniker not in mothers_set:
        mothers_set.add(mother_moniker)
    
    if father_moniker not in fathers_set:
        fathers_set.add(father_moniker)

# Determine the total number of generations
total_generations = max([int(genome_moniker.split('-')[0]) for genome_moniker in person_dict.keys()])

# Calculate the number of generations to consider based on the total number of generations
if total_generations <= 10:
    num_generations_to_consider = 2
else:
    num_generations_to_consider = max(int(total_generations * 0.0025), 3)

# Traverse the person_dict to identify living descendants within the last few generations
for genome_moniker, person in person_dict.items():
    if person['generation'] >= total_generations - num_generations_to_consider + 1:
        living_descendants.add(genome_moniker)


# Perform a depth-first search starting from the living descendants
for genome_moniker in living_descendants:
    dfs_ancestors(genome_moniker, living_ancestors)

# Create a Graphviz Digraph object
dot = graphviz.Digraph(comment='Family Tree')

# Generate the DOT file content with highlighting for living descendants and their ancestors
for genome_moniker, person in person_dict.items():
    name = person['name']
    mother_name = person['mother_name']
    mother_moniker = person['mother_moniker']
    father_name = person['father_name']
    father_moniker = person['father_moniker']
    generation = person['generation']
    
    generation_level = get_generation_level(generation, total_generations)

    node_options = {'color':'lightgrey'}
    egg_options = {'color':'lightgreen','shape':'ellipse'}
    node_options_gen = {'color':'yellow', 'shape':'rect', 'fillcolor':'brightyellow'}
    edge_options_ma = {'color':'deeppink'}
    edge_options_pa = {'color':'blue'}
    
    if genome_moniker in living_descendants:
        node_options['shape'] = 'doublecircle'
        node_options['style'] = 'filled'
        node_options['fillcolor'] = 'lightblue'
    elif genome_moniker in living_ancestors:
        node_options['style'] = 'filled'
        node_options['fillcolor'] = 'lightgrey'
    else:
        node_options['shape'] = 'sqaure'
        node_options['fontcolor'] = 'grey'
    
    if genome_moniker in mothers_set:
        node_options['color'] = 'deeppink'
    
    if genome_moniker in fathers_set:
        node_options['color'] = 'blue'
    
    if name != 'Unknown':
        if not dot.node(genome_moniker):
            dot.node(genome_moniker, name, **node_options)
    elif show_eggs:
        if not dot.node(genome_moniker):
            dot.node(genome_moniker, 'E', **egg_options)

    if name != 'Unknown' or show_eggs is True:
        if mother_moniker != 'unknown':
            if not dot.node(mother_moniker):
                dot.node(mother_moniker, mother_name)
            dot.edge(mother_moniker, genome_moniker, **edge_options_ma)

        if father_moniker != 'unknown':
            if not dot.node(father_moniker):
                dot.node(father_moniker, father_name)
            dot.edge(father_moniker, genome_moniker, **edge_options_pa)
        
        if '.gen' in father_moniker:
            dot.node(father_moniker, father_name, **node_options_gen)

# Render the graph and save it to a file
dot.render(output_file, format='svg', cleanup=True, view=True)
