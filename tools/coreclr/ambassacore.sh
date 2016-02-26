#!/usr/bin/env bash

set -ev

# Fetch CoreCLR
# tests => tests_coreclr
# And much more!

TMP_DIR="/tmp/coreclr_$(date '+%h_%d_%H:%M:%S')"
CORECLR_SOURCE=dotnet_ci
PACKAGE_SOURCE=nuget

error () {
    echo "Error at line $1, sorry!"
    exit 1
}

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
	-l|--latest)
	    
	-a|--archive)
	    CORECLR_SOURCE=archive
	    CORECLR_ARCHIVE="$PWD/$2"
	    
	    shift 2
	    ;;
	-p|--packages)
	    PACKAGE_SOURCE=local
	    PACKAGE_DIR="$PWD/$2"
	    shift 2
	    ;;
	-k|--keep-dir)
	    set KEEP_DIR
	*)
	    echo "Unknown option, sorry!"
	    exit 2
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
	error $LINENO
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

case "$PACKAGE_SOURCE" in
    local)
	cp -r "$PACKAGE_DIR"/* .
	;;
    nuget)
	(echo '<?xml version="1.0" encoding="utf-8"?>
<packages>' &&
		universal_script nuget list -Source https://www.myget.org/F/dotnet-core -Prerelease | tr -d '\r' | awk '/System/{print "<package id=\"" $1 "\" version=\"" $2 "\" />"}' &&
		echo '</packages>') >> packages.config
	mono ~/Downloads/nuget.exe restore -Source https://www.myget.org/F/dotnet-core -PackagesDirectory .
	;;
    *)
	error $LINENO
esac
    
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
    true
    #rm -rf "$TMP_DIR"
fi

