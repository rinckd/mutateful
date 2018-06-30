outlets = 3;
inlets = 3;

function ObservableCallback(id) {
    this.id = id;
    this.name = "<not set>";
}

ObservableCallback.prototype.getCallback = function() {
    var self = this;
    return {
        callback: function(arg) {
            var name = "";
            if (arg.indexOf("name") >= 0) {
                name = arg[arg.indexOf("name") + 1];
                name = name.substr(1, name.length - 2);
            }
            if (name.length > 0 && self.name !== name) {
                post("Name changed! cb called with " + arg + " on id: " + self.id + "\r\n");
                self.name = name;
                outlet(2, parseInt(self.id, 10));
            }
        }
    };
}

ObservableCallback.prototype.setLiveApi = function(api) {
    this.api = api;
}

selectedClipObserver = new LiveAPI(onSelectedClipChanged, "live_set view");
selectedClipObserver.property = "detail_clip";
nameCallback = new ObservableCallback(-1);
var clipNameObserver = {};
var clipContentsObserver = {};
//nameCallback.setLiveApi(clipNameObserver);

clipNotesObserver = [];
notesCallback = new ObservableCallback(-1);
watchedClips = [];

function msg_int(val) {
    post("msg_int called");
    if (inlet === 1 && val > 0) { // piped back from js-object to avoid problems with push/popcontextframe
        var id = "id " + val;
        clipNameObserver.property = "";
        clipNameObserver = null;
        clipNameObserver = new LiveAPI(nameCallback.getCallback().callback, id);
        nameCallback.id = clipNameObserver.id;
        nameCallback.name = getClipName(clipNameObserver);
        clipNameObserver.property = "name";
    } else if (inlet === 2 && val > 0) {
        if (watchedClips[val] !== undefined && watchedClips[val].length !== 0) {
            var currentlyWatchedClips = watchedClips[val];
            var indexesToRemove = [];
            var updatedWatchedClips = [];
            post(currentlyWatchedClips);

            for (var i = 0; i < currentlyWatchedClips.length; i++) {
                var id = currentlyWatchedClips[i];
                var formulaSlot = new LiveAPI("id " + id);
                var formula = getClipName(formulaSlot);
                var referredIds = formulaToReferredIds(formula);
                post("attempting to find id " + val + " in referred ids: " + referredIds + " extra stuff " + referredIds.length + "\r\n");
                if (referredIds.indexOf(val) >= 0) {
                    post("found current clip in referring formula - all is well");

                    var expandedFormula = expandFormula(formula, id);
                    if (expandedFormula) {
                        expandedFormula = "{" + id + "} " + expandedFormula;
                        outlet(0, ["/mu4l/formula/process", expandedFormula]);
                    } else {
                        post("Unable to expand formula for track " + (i + 1) + " clip " + (s + 1) + " - check syntax");
                    }
                } else {
                    post("could not find current clip in referring formula");
                    indexesToRemove.push(id);
                }
            }
            if (indexesToRemove.length > 0) {
                for (i = 0; i < currentlyWatchedClips.length; i++) {
                    if (indexesToRemove.indexOf(i) >= 0) {
                        continue;
                    } else {
                        updatedWatchedClips.push(currentlyWatchedClips[i]);
                    }
                }
                watchedClips[self.id] = updatedWatchedClips;
            }
        }
    }
}

function onSelectedClipChanged(args) {
    if (!args || args.length === 0) return;
    var id = args[args.length - 1];
    if (id === 0) return; // return on empty clip
    post("Selected clip changed, id: " + id + "\r\n");
    // update watchers
    outlet(1, id); // Max does not support creating LiveAPI objects in custom callbacks, so this is handled by piping data back into inlet 2 (see msg_int function above)
}

function getClipName(liveObject) {
    var clipName = liveObject.get("name");
    if (!clipName.length || clipName.length === 0) return "";
    else return clipName[0];
}

function clipRefToId(clipRef) {
    var target = resolveClipReference(clipRef);
    var liveObjectAtClip = getLiveObjectAtClip(target.x, target.y);
    if (!liveObjectAtClip) return;
    return liveObjectAtClip.id;
}

function getClip(trackNo, clipNo) {
    var liveObjectAtClip = getLiveObjectAtClip(trackNo, clipNo);
    if (!liveObjectAtClip) return;
    return getClipData(liveObjectAtClip);
}

function getLiveObjectAtClip(trackNo, clipNo) {
    post("getting track " + trackNo + " clip " + clipNo);
    var liveObject = new LiveAPI("live_set tracks " + trackNo);

    if (!liveObject) {
        post('Invalid liveObject, exiting...');
        return;
    }
    if (liveObject.get('has_audio_input') > 0 && liveObject.get('has_midi_input') < 1) {
        post('Not a midi track!');
        return;
    }
    liveObject.goto("live_set tracks " + trackNo + " clip_slots " + clipNo);

    if (liveObject.get('has_clip') < 1) {
        post("No clip present at track: " + trackNo + 1 + " clip: " + clipNo + 1);
        return;
    }
    liveObject.goto("live_set tracks " + trackNo + " clip_slots " + clipNo + " clip");

    return liveObject;
}

function getClipData(liveObject) {
    var loopStart = liveObject.get('loop_start');
    var clipLength = liveObject.get('length');
    var looping = liveObject.get('looping');
    var data = liveObject.call("get_notes", loopStart, 0, clipLength, 128);
    var result = clipLength + " " + looping + " ";
    for (var i = 2, len = data.length - 1; i < len; i += 6) {
        if (data[i + 5 /* muted */] === 1) {
            continue;
        }
        result += data[i + 1 /* pitch */] + " " + data[i + 2 /* start */] + " " + data[i + 3 /* duration */] + " " + data[i + 4 /* velocity */] + " ";
    }
    post(result);
    return result.slice(0, result.length - 1);  // remove last space
}

// todo: robustify handling of clip references. Track should refer to midi tracks only, filtering out audio tracks. Clip numbers must be checked for overflow wrt number of scenes available.
function setClip(trackNo, clipNo, dataString) {
    post("setClip: " + dataString);
    var data = dataString.split(' ');
    if (data.length < 3)
        return;
    var pathCurrentTrack = "live_set tracks " + trackNo;
    var pathCurrentClipHolder = pathCurrentTrack + " clip_slots " + clipNo;
    var pathCurrentClip = pathCurrentClipHolder + " clip";
    var liveObject = new LiveAPI(pathCurrentTrack);
    if (liveObject.get('has_audio_input') > 0 && liveObject.get('has_midi_input') < 1) {
        post('Not a midi track!');
    }
    var clipLength = data[0];
    var looping = data[1];
    liveObject.goto(pathCurrentClipHolder);
    if (liveObject.get('has_clip') < 1) {
        liveObject.call('create_clip', clipLength);
    }
    liveObject.goto(pathCurrentClip);
    liveObject.set('loop_start', '0');
    liveObject.set('loop_end', clipLength);
    liveObject.call('select_all_notes');
    liveObject.call('replace_selected_notes');
    liveObject.call('notes', (data.length - 2) / 4);
    for (var c = 2; c < data.length; c += 4) {
        liveObject.call('note', data[c], data[c + 1], data[c + 2], data[c + 3], 0);
    }
    liveObject.call('done');
    liveObject.set('looping', looping);
}
function setSelectedClip(dummyTrackNo, dummyClipNo, dataString) {
    post("setSelectedClip: " + dataString);
    var data = dataString.split(' ');
    if (data.length < 3)
        return;
    var pathCurrentClipHolder = "live_set view highlighted_clip_slot";
    var pathCurrentClip = pathCurrentClipHolder + " clip";
    var liveObject = new LiveAPI("live_set view selected_track");
    if (liveObject.get('has_audio_input') > 0 && liveObject.get('has_midi_input') < 1) {
        post('Not a midi track!');
        return;
    }
    var clipLength = data[0];
    var looping = data[1];
    liveObject.goto(pathCurrentClipHolder);
    if (liveObject.get('has_clip') < 1) {
        liveObject.call('create_clip', clipLength);
    }
    liveObject.goto(pathCurrentClip);
    liveObject.set('loop_start', '0');
    liveObject.set('loop_end', clipLength);
    liveObject.call('select_all_notes');
    liveObject.call('replace_selected_notes');
    liveObject.call('notes', (data.length - 2) / 4);
    for (var c = 2; c < data.length; c += 4) {
        liveObject.call('note', data[c], data[c + 1], data[c + 2], data[c + 3], 0);
    }
    liveObject.call('done');
    liveObject.set('looping', looping);
}
function createSceneAndSetClip(trackNo, clipNo, data) {
    var liveObject = new LiveAPI("live_set");
    var numScenes = liveObject.get('scenes').length / 2; // output is of the form id 1, id 2, id 3 and so on, so we divide by 2 to get length
    var index = clipNo;
    if (clipNo >= numScenes) {
        index = -1; // add to end
        clipNo = numScenes;
    }
    liveObject.call('create_scene', index);
    setClip(trackNo, clipNo, data);
}
function enumerate() {
    var liveObject = new LiveAPI("live_set");
    var numScenes = liveObject.get('scenes').length / 2;
    var numTracks = liveObject.get("tracks").length / 2;
    //var trackIxs = [];
    for (var i = 0; i < numTracks; i++) {
        liveObject.goto("live_set tracks " + i);
        if (liveObject.get('has_audio_input') < 1 && liveObject.get('has_midi_input') > 0) {
            for (var s = 0; s < numScenes; s++) {
                liveObject.goto("live_set tracks " + i + " clip_slots " + s);
                if (liveObject.get('has_clip') > 0) {
                    liveObject.goto("live_set tracks " + i + " clip_slots " + s + " clip");
                    var existingName = liveObject.get("name");
                    liveObject.set("name", String.fromCharCode(65 + i) + (s + 1) + existingName);
                }
            }
        }
    }
}
function getSelectedClip() {
    var liveObject = new LiveAPI("live_set view selected_track");
    var result = "";
    if (!liveObject) {
        post('Invalid liveObject, exiting...');
        return;
    }
    if (liveObject.get('has_audio_input') < 1 && liveObject.get('has_midi_input') > 0) {
        liveObject.goto("live_set view highlighted_clip_slot");
        if (liveObject.get('has_clip')) {
            liveObject.goto("live_set view highlighted_clip_slot clip");
            var loopStart = liveObject.get('loop_start');
            var clipLength = liveObject.get('length');
            var looping = liveObject.get('looping');
            var data = liveObject.call("get_notes", loopStart, 0, clipLength, 128);
            result += clipLength + " " + looping + " ";
            for (var i = 2, len = data.length - 1; i < len; i += 6) {
                if (data[i + 5 /* muted */] === 1) {
                    continue;
                }
                result += data[i + 1 /* pitch */] + " " + data[i + 2 /* start */] + " " + data[i + 3 /* duration */] + " " + data[i + 4 /* velocity */] + " ";
            }
        }
        outlet(0, ['/mu4l/selectedclip/get', result.slice(0, result.length - 1 /* remove last space */)]);
        return;
    }
    outlet(0, ['/mu4l/selectedclip/get', ["!"]]);
}



function attachClipObservers() {
    var liveObject = new LiveAPI("live_set"),
        numScenes = liveObject.get('scenes').length / 2,
        numTracks = liveObject.get("tracks").length / 2,
        formulaStartIndex,
        clipName = "",
        formulaStopIndex,
        nameCallback,
        notesCallback;

/*    for (var i = 0; i < liveObservers.length; i++) {
        liveObservers[i].nameObserver.property = "";
        liveObservers[i].contentObserver.property = "";
    }
    liveObservers = [];
*/
    for (var i = 0; i < numTracks; i++) {
        liveObject.goto("live_set tracks " + i);
        if (liveObject.get('has_audio_input') < 1 && liveObject.get('has_midi_input') > 0) {
            for (var s = 0; s < numScenes; s++) {
                liveObject.goto("live_set tracks " + i + " clip_slots " + s);
                if (liveObject.get("has_clip") > 0) { // todo: ...and if name of clip corresponds to a mutate4l command
                    liveObject.goto("live_set tracks " + i + " clip_slots " + s + " clip");
                    clipName = getClipName(liveObject);
                    if (clipName.indexOf("=") >= 0) {
                        var expandedFormula = expandFormula(clipName, liveObject.id);
                        if (expandedFormula) {
                            expandedFormula = "{" + liveObject.id + "} " + expandedFormula;
                            outlet(0, ["/mu4l/formula/process", expandedFormula]);
                        } else {
                            post("Unable to expand formula for track " + (i + 1) + " clip " + (s + 1) + " - check syntax");
                        }

/*                        
                        nameCallback = new ObservableCallback(liveObject.id);
                        notesCallback = new ObservableCallback(liveObject.id);
                        liveObservers[liveObservers.length] = {
                            nameObserver: new LiveAPI(nameCallback.getCallback().callback, liveObject.unquotedpath),
                            contentObserver: new LiveAPI(notesCallback.getCallback().callback, liveObject.unquotedpath)
                        };
                        liveObservers[liveObservers.length - 1].nameObserver.property = "name";
                        liveObservers[liveObservers.length - 1].contentObserver.property = "notes";
                        nameCallback.setLiveApi(liveObservers[liveObservers.length - 1].nameObserver);
                        notesCallback.setLiveApi(liveObservers[liveObservers.length - 1].contentObserver);
*/
                    }
                }
            }
        }
    }

    //nameCallback = new ObservableCallback(-1);
  //  notesCallback = new ObservableCallback(-1);
    /*if (selectedClipObserver !== null) {
        selectedClipObserver.property = "";
        selectedClipObserver = null;
    }*/

//    liveObservers[liveObservers.length - 1].contentObserver.property = "notes";
//    notesCallback.setLiveApi(liveObservers[liveObservers.length - 1].contentObserver);
//    notesCallback.id = liveObservers[liveObservers.length - 1].contentObserver.id;

/*    var highlightedClipCallback = new ObservableCallback(-1);
    var highlightedLiveObject = new LiveAPI(highlightedClipCallback.getCallback().callback, "live_set view highlighted_clip_slot clip");
    highlightedClipCallback.setLiveApi(highlightedLiveObject);
    highlightedClipCallback.id = highlightedLiveObject.id;
    highlightedLiveObject.property = "name";*/
}

function formulaToReferredIds(formula) {
    var clipRefTester = /^([a-z]+\d+)$|^(\*)$/,
        clipRefsFound = false,
        clipRefs = [],
        referredIds = [];

    if (formula.length < 5) return;

    var formulaStartIndex = formula.indexOf("=");
    var formulaStopIndex = formula.indexOf(";");
    if (formulaStartIndex == -1) return; // no valid formula

    if (formulaStopIndex >= 0) {
        formula = formula.substring(formulaStartIndex + 1, formulaStopIndex).toLowerCase();
    } else {
        formula = formula.substring(formulaStartIndex + 1).toLowerCase();
    }
    
    var parts = formula.split(" ");
    for (var i = 0; i < parts.length; i++) { 
        var result = clipRefTester.test(parts[i]); 
        if (!result && clipRefsFound) break;
        if (!result && i == 0) break;
        if (result) {
            clipRefsFound = true;
            clipRefs.push(parts[i]);
        }
    }

    for (i = 0; i < clipRefs.length; i++) {
        var target = resolveClipReference(clipRefs[i]);
        var liveObjectAtClip = getLiveObjectAtClip(target.x, target.y);
        referredIds.push(parseInt(liveObjectAtClip.id, 10));
    }
    return referredIds;
}

function expandFormula(formula, ownId) {
    var clipRefTester = /^([a-z]+\d+)$|^(\*)$/,
        clipRefsFound = false,
        clipRefs = [],
        clipRefsResolved = [],
        expandedFormulaParts = [];

    if (formula.length < 5) return;

    var formulaStartIndex = formula.indexOf("=");
    var formulaStopIndex = formula.indexOf(";");
    if (formulaStartIndex == -1) return; // no valid formula

    if (formulaStopIndex >= 0) {
        formula = formula.substring(formulaStartIndex + 1, formulaStopIndex).toLowerCase();
    } else {
        formula = formula.substring(formulaStartIndex + 1).toLowerCase();
    }
    var parts = formula.split(" ");
    for (var i = 0; i < parts.length; i++) { 
        var result = clipRefTester.test(parts[i]); 
        if (!result && clipRefsFound) break;
        if (!result && i == 0) break;
        if (result) {
            clipRefsFound = true;
            clipRefs.push(parts[i]);
        }
    }

    for (i = 0; i < clipRefs.length; i++) {
        var target = resolveClipReference(clipRefs[i]);
        var liveObjectAtClip = getLiveObjectAtClip(target.x, target.y);

        if (watchedClips[liveObjectAtClip.id] === undefined) {
            watchedClips[liveObjectAtClip.id] = [ownId];
        } else if (watchedClips[liveObjectAtClip.id].indexOf(ownId) < 0) {
            watchedClips[liveObjectAtClip.id].push(ownId);
        }
        post("updated watchedClips for id " + ownId + ": " + watchedClips[liveObjectAtClip.id] + "\r\n");

        var clipData = getClipData(liveObjectAtClip);
        if (!clipData) {
            return;
        }
        expandedFormulaParts.push("[" + clipData + "]");
    }

    for (i = 0; i < parts.length; i++) {
        if (!clipRefTester.test(parts[i])) {
            expandedFormulaParts.push(parts[i]);
        }
    }
    return expandedFormulaParts.join(" ");
}

// todo: remove
function expandFormula2(formula, ownId) {
    var clipRefTester = /^([a-z]+\d+)$|^(\*)$/,
        clipRefsFound = false,
        clipRefs = [],
        clipRefsResolved = [],
        expandedFormulaParts = [];

    if (formula.length < 5) return;

    var formulaStartIndex = formula.indexOf("=");
    var formulaStopIndex = formula.indexOf(";");
    if (formulaStartIndex == -1) return; // no valid formula

    if (formulaStopIndex >= 0) {
        formula = formula.substring(formulaStartIndex + 1, formulaStopIndex).toLowerCase();
    } else {
        formula = formula.substring(formulaStartIndex + 1).toLowerCase();
    }
    var parts = formula.split(" ");
    for (var i = 0; i < parts.length; i++) { 
        var result = clipRefTester.test(parts[i]); 
        if (!result && clipRefsFound) break;
        if (!result && i == 0) break;
        if (result) {
            clipRefsFound = true;
            clipRefs.push(parts[i]);
        }
    }
    for (i = 0; i < clipRefs.length; i++) {
        var target = resolveClipReference(clipRefs[i]);
        var liveObjectAtClip = getLiveObjectAtClip(target.x, target.y);
        post(liveObjectAtClip.id + " ownid " + ownId + "watchedClips index " + watchedClips[liveObjectAtClip.id].indexOf(ownId) + "\r\n");
        if (watchedClips[liveObjectAtClip.id] === undefined) {
            watchedClips[liveObjectAtClip.id] = [ownId];
        } else if (watchedClips[liveObjectAtClip.id].indexOf(ownId) < 0) {
            watchedClips[liveObjectAtClip.id].push(ownId);
        }
        post("updated watchedClips for id " + ownId + ": " + watchedClips[liveObjectAtClip.id] + "\r\n");

        var clipData = getClipData(liveObjectAtClip);
        if (!clipData) {
            return;
        }
        expandedFormulaParts.push("[" + clipData + "]");
    }

    for (i = 0; i < parts.length; i++) {
        if (!clipRefTester.test(parts[i])) {
            expandedFormulaParts.push(parts[i]);
        }
    }
    return expandedFormulaParts.join(" ");
}

function resolveClipReference(reference) {
    var channel = "", clip = "", c = 0;

    channel = reference[c++].toLowerCase();
    while (c < reference.length && isNumeric(reference[c]))
        clip += reference[c++];

    return {
        x: channel.charCodeAt(0) - "a".charCodeAt(0),
        y: parseInt(clip) - 1
    };
}

function isNumeric(c) {
    return c.length == 1 && c >= '0' && c <= '9';
}

function isAlpha(c) {
    return c.length == 1 && (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
}