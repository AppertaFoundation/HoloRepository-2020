import logging
import argparse
from argparse import RawTextHelpFormatter
from core.pipelines.pipelines_controller import (
    get_pipeline_description,
    load_pipeline_dynamically,
)
from models.model_controller import get_seg_types, get_file_types, get_proc_seg_types


plid = "kidney_segmentation"


def get_description():
    pipeline_description = get_pipeline_description(plid)

    model__file_types = get_file_types(plid)

    return (
        pipeline_description
        + "\n"
        + "Input file must be of type "
        + ", ".join(model__file_types)
        + "\nDefault segmentaion: kidneys & tumor "
    )


def add_parser_arguments(parser):
    model_seq_types = get_seg_types(plid)
    model_proc_seg_types = get_proc_seg_types(plid)

    parser.add_argument(
        "input",
        metavar="i",
        type=str,
        help="Specify the path to the directory containing the input scans",
    )
    parser.add_argument(
        "output",
        metavar="o",
        type=str,
        help="Specify the path of a single output file in the form of a glb file",
    )
    parser.add_argument(
        "-type",
        metavar="t",
        type=int,
        nargs="*",
        default=[1, 2],
        choices=range(0, len(model_seq_types)),
        help="Specify the type of brain segmentation through an integer. Multiple integers can be supplied"
        + "\n"
        + "Segmentation types include "
        + model_proc_seg_types,
    )
    parser.add_argument(
        "-q",
        "--quiet",
        action="store_true",
        help="Set the logging level from INFO to ERROR",
    )
    parser.set_defaults(which=plid)


def run(args):
    logging.info("Loading and initializing kidney pipeline dynamically")
    input_dir = args.input
    output_path = args.output
    segment_type = args.type
    pipeline_module = load_pipeline_dynamically(plid)
    pipeline_module.run(input_dir, output_path, segment_type)
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
