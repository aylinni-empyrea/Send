# Send
Just send text.

## /send
**Permission:** *send*

**Usage:** `/send (color) target message`
**Example:** `/send 255,100,100 newy hi`
**Result:**
```
hi
```

## /sendbc
**Permission:** *send.broadcast*

Just like `/send`, but broadcasts instead.

**Usage:** `/send (color) message`
**Example:** `/send 255,100,100 hi everyone`
**Result:**
```
hi everyone
```

### /sendas
**Permission:** *send.broadcast.impersonate*

A variation of `/sendbc`, this command mimics the player's chat.

Just like `/send`, but broadcasts instead.

**Usage:** `/send (color) playertomimic message`
**Example:** `/send 255,100,100 newy hi everyone`
**Result:**
```
(Admin) Newy: hi everyone
```
**!** Doesn't work with offline players for now; will be fixed in later release.
