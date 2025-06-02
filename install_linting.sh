#!/bin/bash

set -eu

SLN_ROOT=$1

cp Directory.Build.props.template $SLN_ROOT/Directory.Build.props
cat .editorconfig.template >> $SLN_ROOT/.editorconfig
if [ ! -f "$SLN_ROOT/lint.sh" ]; then
    cp lint.sh $SLN_ROOT/
fi
pushd $SLN_ROOT
dotnet new tool-manifest
dotnet tool install csharpier
popd
