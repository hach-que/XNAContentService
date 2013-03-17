#!/bin/bash

LAST_COMMIT=$(git log -1 --pretty=%B)

if [ "${LAST_COMMIT:0:8}" == "[XNACCS]" ]; then
    echo "Not performing commit; last commit was by the content compilation service."
    exit 0
fi

git add compiled/*
git commit -m "[XNACCS] Processed commit '$LAST_COMMIT'" --author "XNA Content Compiler Service <xnaccs@build.redpointsoftware.com.au>"
git push
