#!/bin/bash

# MGFXC for reference!
# https://docs.monogame.net/articles/getting_started/tools/mgfxc.html

OUTPUT_EXTENSION=".xnb"
TARGET=$1
PROFILE=$2
SCRIPT_DIR=$(dirname -- "$(realpath -- "$0")")
OUTPUT_DIR="$SCRIPT_DIR"

# exit early if MGFXC isn't found
if ! command -v mgfxc &> /dev/null; then
	echo "ERROR: MGFXC not found on system!"
	exit 1
fi

# profile checking!
PROFILE_SUFFIX=""
if [[ $PROFILE == "OpenGL" ]]; then
	PROFILE_SUFFIX="gl"
elif [[ $PROFILE == "DirectX_11" ]]; then
	PROFILE_SUFFIX="dx"
else
	echo "ERROR: Invalid shader profile '$PROFILE'!"
	exit 1
fi

mkdir -p $OUTPUT_DIR

compile_shader() {
	# add an escape backslash before all slashes so it works with sed later
	REGEX_COMPATIBLE_PATH=$(echo $OUTPUT_DIR | sed 's/[\/\\]/\\\//g')

	# remove output dir from path while keeping sub directories
	PATH_REMOVED_FILE=$(echo $1 | sed "s/^$REGEX_COMPATIBLE_PATH//")

	# re-add output dir and replace extension
	OUTPUT=$OUTPUT_DIR/$(echo $PATH_REMOVED_FILE | sed "s/\.fx$/\_$PROFILE_SUFFIX$OUTPUT_EXTENSION/")

	rm -f $OUTPUT

	echo "compiling '$1' into '$OUTPUT'..."

	mgfxc $1 $OUTPUT /Profile:$PROFILE
}

if [[ $TARGET == "all" ]]; then
	echo "compiling all shaders..."

	for file in $(find "$SCRIPT_DIR/" -type f -name "*.fx"); do
		compile_shader $file
	done
else
	compile_shader $TARGET
fi

echo "shaders compiled :D"
