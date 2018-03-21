The Thrift netcore project was not on Nuget and a bunch of random different packages existed for it out there. I'd rather build from the source of truth, so I decided to go the route of having a subfolder in this repo. I didn't want a submodule as I did not want to pull in the whole Thrift repo. To do this, I followed the steps from https://gist.github.com/tswaters/542ba147a07904b1f3f5. Here are the specific steps I took:

## Initial setup...

```sh
# add thrift remote, create new tracking branch, 
git remote add -f thrift git@github.com:apache/thrift.git
git checkout -b thrift-master thrift/master

# split off subdir of thrift repo into separate branch
git subtree split -q --squash --prefix=lib/netcore --annotate="[thrift] " --rejoin -b merging/thrift

# add separate branch as subdirectory on master.
git checkout master
git subtree add --prefix=src/Thrift/netcore --squash merging/thrift
```

## Fetching upstram
```sh
# switch back to tracking branch, fetch & rebase.
git checkout thrift-master
git pull thrift/master

# update the separate branch with changes from upstream
git subtree split -q --prefix=lib/netcore --annotate="[thrift] " --rejoin -b merging/thrift

# switch back to master and use subtree merge to update the subdirectory
git checkout master
git subtree merge -q --prefix=src/Thrift/netcore --squash merging/thrift
```
