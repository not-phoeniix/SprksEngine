#!/bin/bash

# MGFXC for reference!
# https://docs.monogame.net/articles/getting_started/tools/mgfxc.html

OUTPUT_EXTENSION=".xnb"
PROFILE=$1
SCRIPT_DIR=$(dirname -- "$(realpath -- "$0")")
OUTPUT_DIR="$SCRIPT_DIR/PrecompiledBinaries"

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

echo "removing files inside output dir '$OUTPUT_DIR'..."

rm -f $(find $OUTPUT_DIR/ -type f -name "*.xnb")

echo "compiling shaders..."

for file in $(find "$SCRIPT_DIR/" -type f -name "*.fx"); do
	OUTPUT=$OUTPUT_DIR/$(echo $(basename -- $file) | sed "s/\.fx$/\_$PROFILE_SUFFIX$OUTPUT_EXTENSION/")

	echo "compiling '$file' into '$OUTPUT'..."
	# echo "mgfxc $file $OUTPUT /Profile:$PROFILE"

	mgfxc $file $OUTPUT /Profile:$PROFILE
done

echo "shaders compiled :D"

