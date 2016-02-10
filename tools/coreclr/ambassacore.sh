#!/usr/bin/env bash

set -e

# Fetch CoreCLR
# tests => tests_coreclr
# And much more!

TMP_DIR="/tmp/coreclr_$(date '+%h_%d_%H:%M:%S')"
CORECLR_SOURCE=dotnet_ci

if [ "z$HOSTTYPE" != zx86_64 ]; then
   echo 64 bit only, sorry!
   exit 1
fi

case "$OSTYPE" in
    darwin*)
	OS=OSX
	;;
    *)
	echo "Unsupported OS $OSTYPE, sorry!"
	exit 1
	;;
esac

while [[ $# > 1 ]]; do
    case "$1" in
	-a|--archive)
	    CORECLR_SOURCE=archive
	    CORECLR_ARCHIVE="$PWD/$2"
	    shift 2
	    ;;
	*)
	    echo "Unknown option, sorry!"
	    ;;
    esac
done

echo Temporary directory: "$TMP_DIR"

mkdir "$TMP_DIR"
pushd "$TMP_DIR"

# Step 1. Fetch the runtime.
case "$CORECLR_SOURCE" in
    dotnet_ci)
	curl http://dotnet-ci.cloudapp.net/job/dotnet_coreclr/job/release_osx/688/artifact/*zip*/archive.zip -o archive.zip
	CORECLR_ARCHIVE=archive.zip
	;;
    archive)
	;;
    *)
	echo ...sorry.
	exit 1
	;;
esac

unzip "$CORECLR_ARCHIVE"
	
pushd "archive/bin/Product/$OS.x64.Release"

chmod +x corerun

# Step 2. Fetch the (mostly)facade packages.

# Make the Script tool behave the same.
# We need script because NuGet does not like UNIX pipes.
universal_script () {
    case "$OS" in
	OSX)
	    script -q /dev/null "$@"
	    ;;
	Linux)
	    # TODO: escape input
	    script -qc "$*" /dev/null
	    ;;
	*)
	    echo "Sorry, don't know how to use `script` on this architecture"
	    ;;
    esac
}

mkdir packages
pushd packages
(echo '<?xml version="1.0" encoding="utf-8"?>
<packages>' &&
	universal_script nuget list -Source https://www.myget.org/F/dotnet-core -Prerelease &&
	echo '</packages>') >> packages.config
popd
	


# In theory, we could also use the HTTP WCF API directly, something along these lines...
# curl -s "https://dotnet.myget.org/F/dotnet-core/Search()?$filter=IsAbsoluteLatestVersion&$orderby=Id&$skip=0&$top=30&searchTerm=''&targetFramework=''&includePrerelease=true" | | xmllint --format - | gsed -En 's/.*<d:Id>(.*)<\/d:Id>.*<d:Version>(.*)<\/d:Version>/<package id=\"\1\" version= \2/gp'
# Note that skip/top don't in the line above don't seem to affect the results for whatever reason, so something must be missing.

ls

popd
popd

if [ "${TMP_DIR:0:4}" != "/tmp" ]; then
    echo Something horrible has just almost happened. Probably.
else
    #rm -rf "$TMP_DIR"
fi

