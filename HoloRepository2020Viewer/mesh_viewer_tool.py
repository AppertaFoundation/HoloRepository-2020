import argparse
from argparse import RawTextHelpFormatter
import logging

from core.pipelines.pipelines_controller import (
    get_pipeline_description,
    load_pipeline_dynamically,
)
from models.model_controller import (
    get_seg_types,
    get_file_types,
    get_req_modalities,
    get_proc_seg_types,
)

import logging

plid = "glb_importer"


def get_description():
    pipeline_description = get_pipeline_description(plid)
    model_file_types = get_file_types(plid)
    model_req_mods = get_req_modalities(plid)

    return (
        pipeline_description
        + "\n3 input scans (modalities) required of types "
        + ", ".join(model_req_mods)
        + "\nInput files must be of type "
        + " or ".join(model_file_types)
        + "\nDefault segmentaion: cortical_gray_matter "
    )


def add_parser_arguments(parser):
    parser.add_argument(
        "input_file",
        metavar="input_file",
        type=str,
        help="Specify the path to the directory containing the T2-FLAIR input scans",
    )
    parser.add_argument(
        "-q",
        "--quiet",
        action="store_true",
        help="Set the logging level from INFO to ERROR",
    )
    parser.set_defaults(which=plid)


def run(args):
    logging.info("Loading and initializing mesh viewer dynamically")
    input_file = args.input_file
    pipeline_module = load_pipeline_dynamically(plid)
    pipeline_module.run(input_file)
    logging.info("Done.")


def main():
    description = get_description()
    parser = argparse.ArgumentParser(
        description=description, formatter_class=RawTextHelpFormatter
    )
    add_parser_arguments(parser)
    args = parser.parse_args()
    logging.basicConfig(
        level=logging.ERROR if args.quiet else logging.INFO,
        format="%(asctime)s - %(module)s:%(levelname)s - %(message)s",
        datefmt="%d-%b-%y %H:%M:%S",
    )
    run(args)


if __name__ == "__main__":
    main()
