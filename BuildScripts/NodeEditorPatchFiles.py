import os
import shutil


print('Starting Node Editor Build Script --  {}'.format((__file__)))

# there are 2 FILES that we need to patch. 

# let's get the file BaseNodeBlock.cs
output_file_baseblock = '{}{}'.format(os.getcwd(), "\\..\\..\\..\\NodeEditor\\NodeEditor_Windows\\BaseNodeBlock.cs")

with open(output_file_baseblock) as file:
    file_lines = file.readlines()
    line_count = 0
    while line_count < len(file_lines):
        if(file_lines[line_count].find("public abstract partial class BaseNodeBlock") != -1):
            file_lines[line_count] = file_lines[line_count].replace("/*", "")
            file_lines[line_count] = file_lines[line_count].replace("*/", "")
        line_count += 1

with open(output_file_baseblock, 'w') as fp:
    for item in file_lines:
        # write each item on a new line
        fp.write("%s" % item)

# --------------------------------------------------------------------------------------------------------------------------------------
        
# at this point we have patched the first file. Now let's do the last one. ConnectionNode.cs
output_file_connection = '{}{}'.format(os.getcwd(), "\\..\\..\\..\\NodeEditor\\NodeEditor_Windows\\ConnectionNode.cs")

with open(output_file_connection) as file:
    file_lines = file.readlines()
    line_count = 0
    while line_count < len(file_lines):
        if(file_lines[line_count].find("using System.Collections.Generic;") != -1):
            
            file_lines.insert(line_count+1, "using BixBite.NodeEditor;")
            file_lines.insert(line_count+1, "using System.Windows.Shapes;")
            file_lines.insert(line_count+1, "using System.Windows.Controls;")
            file_lines.insert(line_count+1, "using System.Text;")
            file_lines.insert(line_count+1, "using System.Linq;")
            
        if(file_lines[line_count].find("public partial class ConnectionNode ") != -1):
            file_lines[line_count] = file_lines[line_count].replace("//", "") # remove comment
            
        if(file_lines[line_count].find("public List<Path> Curves = new List<Path>();") != -1):
            file_lines[line_count] = file_lines[line_count].replace("//", "") # remove comment
            
        line_count += 1

with open(output_file_connection, 'w') as fp:
    for item in file_lines:
        # write each item on a new line
        fp.write("%s" % item)

print('End of Node Editor Build Script --  {}'.format((__file__)))