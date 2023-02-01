import os
import sys
import shutil
import glob
from os.path import join, isfile
from shutil import copytree

# def GetCodeHeaderStamp(outputArray : String[]):

# def copy_files_recrusively(src, dest):
#     for file_path in glob.glob(os.path.join(src, '**', '*.cs'), recursive=True):
#         new_path = os.path.join(dest, os.path.basename(file_path))
#         shutil.copy(file_path, new_path)
        
def copy_files_recrusively(src, dest):
    # ignore any files but files with '.h' extension
    ignore_func = lambda d, files: [f for f in files if isfile(join(d, f)) and f[-2:] == '.cs']
    copytree(src, dest, ignore=ignore_func)

print('Starting Node Editor Build Script --  {}'.format((__file__)))

# we need to get all the files from Bixbite, and copy them to NodeEditor/Bixbite_windows
output_path = '{}{}'.format(os.getcwd(), "\\..\\..\\..\\NodeEditor\\NodeEditor_Windows\\")

# step 1. Delete the desired folder.
if os.path.exists(output_path) and os.path.isdir(output_path):
    # print("Folder exists")
    shutil.rmtree(output_path)

# step 2. Create the folders again nice and clean
# os.makedirs(output_path)
# os.makedirs('{}{}'.format(output_path, "\\Resources"))

# step 3. Copy all the bixbite files.
copy_files_recrusively('{}{}'.format(os.getcwd(), "\\..\\..\\..\\Bixbite\\NodeEditor\\"), output_path)
copy_files_recrusively('{}{}'.format(os.getcwd(), "\\..\\..\\..\\Bixbite\\Resources"), '{}{}'.format(output_path, "Resources"))


print('End of Node Editor Build Script --  {}'.format((__file__)))