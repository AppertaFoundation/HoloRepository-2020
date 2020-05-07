import os
import subprocess
import platform

path_to_ar_viewer = r"path\to\ar_viewer"

plid_to_id_dict = {
    "lung_segmentation": "lung",
    "abdominal_segmentation": "abdomen",
    "kidney_segmentation": "kidney",
}


def is_supported(plid):
    return (plid in plid_to_id_dict) and (platform.system()=='Windows')


def start_viewer(input_model_path, plid):
    ar_path = os.path.abspath(path_to_ar_viewer)
    file_path = os.path.abspath(input_model_path)
    model = plid_to_id_dict[plid]
    command = [ar_path, "-file", file_path, "-organ", model]
    print(command)
    subprocess.call(command)
