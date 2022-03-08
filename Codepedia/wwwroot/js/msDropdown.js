var Tte;
var wa = new Promise(e=>Tte = e);
let Yt = (e,t)=>{
    wa.then(o=>o.ct.capturePageAction(e, t))
};
var Ee = {
    CLICKLEFT: "CL",
    CLICKRIGHT: "CR",
    CLICKMIDDLE: "CM",
    SCROLL: "S",
    ZOOM: "Z",
    RESIZE: "R",
    KEYBOARDENTER: "KE",
    KEYBOARDSPACE: "KS",
    GAMEPADA: "CGA",
    GAMEPADMENU: "CGM",
    OTHER: "O",
    AUTO: "A"
}, he = {
    left: 37,
    up: 38,
    right: 39,
    down: 40,
    home: 36,
    end: 35,
    escape: 27,
    enter: 13,
    space: 32,
    eight: 56,
    numPadAsterisk: 106,
    a: 65,
    b: 90
}, ue = {
    UNDEFINED: 0,
    NAVIGATIONBACK: 1,
    NAVIGATION: 2,
    NAVIGATIONFORWARD: 3,
    APPLY: 4,
    REMOVE: 5,
    SORT: 6,
    EXPAND: 7,
    REDUCE: 8,
    CONTEXTMENU: 9,
    TAB: 10,
    COPY: 11,
    EXPERIMENTATION: 12,
    PRINT: 13,
    SHOW: 14,
    HIDE: 15,
    MAXIMIZE: 16,
    MINIMIZE: 17,
    BACKBUTTON: 18,
    STARTPROCESS: 20,
    PROCESSCHECKPOINT: 21,
    COMPLETEPROCESS: 22,
    SCENARIOCANCEL: 23,
    DOWNLOADCOMMIT: 40,
    DOWNLOAD: 41,
    SEARCHAUTOCOMPLETE: 60,
    SEARCH: 61,
    SEARCHINITIATE: 62,
    TEXTBOXINPUT: 63,
    PURCHASE: 80,
    ADDTOCART: 81,
    VIEWCART: 82,
    ADDWISHLIST: 83,
    FINDSTORE: 84,
    CHECKOUT: 85,
    REMOVEFROMCART: 86,
    PURCHASECOMPLETE: 87,
    VIEWCHECKOUTPAGE: 88,
    VIEWCARTPAGE: 89,
    VIEWPDP: 90,
    UPDATEITEMQUANTITY: 91,
    INTENTTOBUY: 92,
    PUSHTOINSTALL: 93,
    SIGNIN: 100,
    SIGNOUT: 101,
    SOCIALSHARE: 120,
    SOCIALLIKE: 121,
    SOCIALREPLY: 122,
    CALL: 123,
    EMAIL: 124,
    COMMUNITY: 125,
    SOCIALFOLLOW: 126,
    VOTE: 140,
    SURVEYINITIATE: 141,
    SURVEYCOMPLETE: 142,
    REPORTAPPLICATION: 143,
    REPORTREVIEW: 144,
    SURVEYCHECKPOINT: 145,
    CONTACT: 160,
    REGISTRATIONINITIATE: 161,
    REGISTRATIONCOMPLETE: 162,
    CANCELSUBSCRIPTION: 163,
    RENEWSUBSCRIPTION: 164,
    CHANGESUBSCRIPTION: 165,
    REGISTRATIONCHECKPOINT: 166,
    CHATINITIATE: 180,
    CHATEND: 181,
    TRIALSIGNUP: 200,
    TRIALINITIATE: 201,
    SIGNUP: 210,
    FREESIGNUP: 211,
    PARTNERREFERRAL: 220,
    LEARNLOWFUNNEL: 230,
    LEARNHIGHFUNNEL: 231,
    SHOPPINGINTENT: 232,
    VIDEOSTART: 240,
    VIDEOPAUSE: 241,
    VIDEOCONTINUE: 242,
    VIDEOCHECKPOINT: 243,
    VIDEOJUMP: 244,
    VIDEOCOMPLETE: 245,
    VIDEOBUFFERING: 246,
    VIDEOERROR: 247,
    VIDEOMUTE: 248,
    VIDEOUNMUTE: 249,
    VIDEOFULLSCREEN: 250,
    VIDEOUNFULLSCREEN: 251,
    VIDEOREPLAY: 252,
    VIDEOPLAYERLOAD: 253,
    VIDEOPLAYERCLICK: 254,
    VIDEOVOLUMECONTROL: 255,
    VIDEOAUDIOTRACKCONTROL: 256,
    VIDEOCLOSEDCAPTIONCONTROL: 257,
    VIDEOCLOSEDCAPTIONSTYLE: 258,
    VIDEORESOLUTIONCONTROL: 259,
    VIRTUALEVENTJOIN: 260,
    VIRTUALEVENTEND: 261,
    IMPRESSION: 280,
    CLICK: 281,
    RICHMEDIACOMPLETE: 282,
    ADBUFFERING: 283,
    ADERROR: 284,
    ADSTART: 285,
    ADCOMPLETE: 286,
    ADSKIP: 287,
    ADTIMEOUT: 288,
    OTHER: 300
};

//

const g_e = /([a-z]\.)([a-z])/gi
    , f_e = /([a-z])([A-Z]+[a-z])/g
    , h_e = /(\w\/)(\S?)/gi
    , xb = "<wbr>"
    , gw = `$1${xb}$2`
    , b_e = /\u200B/g;
function uo(e, t=2 | 1) {
    return !e || !e.length || t === 0 || (t & 2 && (e = e.replace(g_e, gw)),
    t & 1 && (e = e.replace(f_e, gw)),
    t & 4 && (e = e.replace(h_e, gw))),
    e
}
const v_e = {
    "&": "&amp;",
    "<": "&lt;",
    ">": "&gt;",
    '"': "&quot;",
    "'": "&#39;"
}, Nee = /[&<>"']/g, __e = RegExp(Nee.source);
function Ce(e) {
    return e && __e.test(e) ? e.replace(Nee, t=>v_e[t]) : e
}
const y_e = /(^|\s)(C#|F#|C\+\+)($|\s|[.,!?;:])/g;
function bw(e, t="text") {
    let o = `$1$2${t === "text" ? "\u200E" : "&lrm;"}$3`;
    return e.replace(y_e, o)
}
function Qf(e) {
    let t = Object.assign({}, e);
    delete t.children,
    delete t.isNewSection,
    delete e.url,
    delete e.href,
    e.children.unshift(t)
}
var wR = class {
    constructor(r) {
        this.isRtl = r
    }
    hasChildren(t) {
        return !!t.children
    }
    children(t) {
        return t.url && Qf(t),
        t.children
    }
    textTitle(t) {
        return this.isRtl ? bw(t.toc_title, "text") : t.toc_title
    }
    htmlTitle(t) {
        return uo(Ce(this.isRtl ? bw(t.toc_title, "html") : t.toc_title), 2)
    }
    href(t) {
        return t.url;
        //if (t.url.external)
        //    return t.url.href;
        //let {origin: o, pathname: n, search: r, hash: s} = t.url;
        //return location.hostname === "localhost" && /\/$/.test(n) && (n += "index"),
        //o + n + r + s
    }
    isNewSection(t) {
        return !!t.isNewSection
    }
    isExpanded(t) {
        return !!t.expanded
    }
    isSelected(t) {
        return !!t.selected
    }
    setHtmlAttributes(t, o) {
        //t.monikers && t.monikers.length && o("data-moniker", t.monikers.join(" "))
    }
}

// Wce([], new wR(false), 'Table of contents')

function Wce(e, t, o) {
    return Yce(e, t, o, !0)
}
function Kce(e, t, o) {
    return Yce(e, t, o, !1)
}
function zA(e) {
    let t = e.querySelector(".tree-item.is-selected");
    t || (t = np(e) ? e.querySelector(".tree-item") : e.querySelector(".tree-item.is-leaf")),
    t && op(t)
}
function FA(e, t) {
    e.addEventListener("focus", EIe, !0),
    e.addEventListener("click", o=>kIe(o, t), !0),
    e.addEventListener("keydown", o=>AIe(o, t), !0)
}
function Yce(e, t, o, n) {
    let r = document.createElement("ul");
    return r.classList.add("tree"),
    r.setAttribute("role", "tree"),
    r.setAttribute("aria-label", o),
    r.setAttribute("data-bi-name", "tree"),
    r.setAttribute("data-is-collapsible", n ? "true" : "false"),
    Jce(r, e, t),
    zA(r),
    FA(r, t),
    r
}
function Jce(e, t, o, n=1, r) {
    let s = np(e)
      , i = 1;
    for (let a of t) {
        let l = document.createElement("li")
          , c = document.createElement("a");
        e.appendChild(l),
        o.isNewSection(a) && l.classList.add("border-top"),
        o.setHtmlAttributes(a, (p,m)=>l.setAttribute(p, m));
        let d = o.htmlTitle(a);
        if (o.hasChildren(a)) {
            let p = (r ? `${r}_` : "title-") + `${i}-${n}`;
            jce(l, a),
            l.classList.add("tree-item"),
            l.setAttribute("aria-setsize", t.length.toString()),
            l.setAttribute("aria-level", n.toString()),
            l.setAttribute("aria-posinset", i.toString()),
            l.setAttribute("role", "treeitem"),
            l.setAttribute("tabindex", "-1"),
            l.setAttribute("id", p),
            s && l.setAttribute("aria-expanded", "false");
            let m = document.createElement("span");
            if (l.appendChild(m),
            m.setAttribute("data-bi-name", "tree-expander"),
            s) {
                m.className = "tree-expander";
                let f = document.createElement("span");
                m.appendChild(f),
                f.className = "tree-expander-indicator docon docon-chevron-right-light",
                f.setAttribute("aria-hidden", "true")
            }
            m.insertAdjacentHTML("beforeend", d),
            (!s || o.isExpanded(a)) && tp(l, !0, o),
            i++;
            continue
        }
        c.setAttribute("aria-setsize", t.length.toString()),
        c.setAttribute("aria-level", n.toString()),
        c.setAttribute("aria-posinset", i.toString()),
        c.setAttribute("role", "treeitem"),
        c.setAttribute("tabindex", "-1"),
        !s && e.parentElement && e.parentElement.id && c.setAttribute("aria-describedby", e.parentElement.id),
        l.setAttribute("role", "none"),
        jce(c, a),
        l.appendChild(c),
        c.classList.add("tree-item", "is-leaf", "has-external-link-indicator"),
        c.setAttribute("data-bi-name", "tree-leaf"),
        c.href = o.href(a),
        c.innerHTML = d,
        o.isSelected(a) && (c.classList.add("is-selected"),
        c.setAttribute("aria-current", "page")),
        c.querySelector(".icon") != null && c.classList.add("has-icon"),
        i++
    }
}
function jce(e, t) {
    e.node = t
}
function Xce(e) {
    return e.node
}
function np(e) {
    return e.closest(".tree").getAttribute("data-is-collapsible") !== "false"
}
function Zce(e) {
    return e.getAttribute("aria-expanded") === "true" || !np(e)
}
function tp(e, t, o) {
    let n = e.getAttribute("aria-level")
      , s = (n ? parseInt(n, 10) : 1) + 1
      , i = np(e);
    if (!i && !t || (i && e.setAttribute("aria-expanded", t.toString()),
    e.classList[t ? "add" : "remove"]("is-expanded"),
    !t || e.lastElementChild instanceof HTMLUListElement))
        return;
    let a = Xce(e)
      , l = document.createElement("ul");
    l.classList.add("tree-group"),
    l.setAttribute("role", "group"),
    e.appendChild(l);
    let c = e.getAttribute("id")
      , d = o.children(a);
    Jce(l, d, o, s, c)
}
function op(e) {
    let t = e.closest(".tree");
    Array.from(t.querySelectorAll('[tabindex="0"]')).forEach(o=>o.setAttribute("tabindex", "-1")),
    e.setAttribute("tabindex", "0")
}
function Qce(e, t) {
    let o = e.closest(".tree")
      , r = np(e) ? ':not([aria-expanded="false"]) [role="treeitem"]' : '[role="treeitem"] .is-leaf'
      , s = Array.from(o.querySelectorAll(r));
    t === "preceding" && s.reverse();
    let i = t === "preceding" ? Node.DOCUMENT_POSITION_PRECEDING : Node.DOCUMENT_POSITION_FOLLOWING;
    return s.find(a=>e.compareDocumentPosition(a) & i && a.closest('.tree [aria-expanded="false"] [role="treeitem"]') !== a && eue(a.closest("li")))
}
function eue(e) {
    return window.getComputedStyle(e).display !== "none"
}
function EIe({target: e}) {
    let t = e instanceof HTMLElement && e.closest('[role="treeitem"]');
    !t || op(t)
}
function kIe({target: e}, t) {
    let {REDUCE: o, EXPAND: n} = ue
      , {CLICKLEFT: r} = Ee
      , s = e instanceof HTMLElement && e.closest(".tree-expander, a");
    if (!s)
        return;
    let i;
    if (s instanceof HTMLAnchorElement)
        i = s;
    else {
        i = s.parentElement;
        let c = Zce(i);
        tp(i, !c, t),
        Yt(e, {
            behavior: c ? o : n,
            actionType: r
        })
    }
    let a = Xce(i)
      , l = new CustomEvent("tree-item-clicked",{
        detail: a,
        bubbles: !0
    });
    s.closest(".tree").dispatchEvent(l)
}
function AIe(e, t) {
    let {target: o, keyCode: n, shiftKey: r, altKey: s, ctrlKey: i} = e
      , {REDUCE: a, EXPAND: l} = ue
      , {KEYBOARDENTER: c, KEYBOARDSPACE: d, OTHER: p} = Ee;
    if (s || i || r && n !== he.eight && !(n >= he.a && n <= he.b))
        return;
    let m = o instanceof HTMLElement && o.closest('[role="treeitem"]');
    if (!m)
        return;
    let f = m instanceof HTMLAnchorElement
      , g = !f && Zce(m)
      , v = np(m);
    if (n === he.enter || n === he.space) {
        if (f || !v)
            return;
        tp(m, !g, t),
        Yt(e.target, {
            behavior: g ? a : l,
            actionType: n === he.enter ? c : d
        }),
        e.preventDefault();
        return
    }
    if (n === he.right) {
        if (f || !v)
            return;
        if (g) {
            let b = m.querySelector('[role="treeitem"]');
            b.focus(),
            op(b)
        } else
            tp(m, !0, t),
            Yt(e.target, {
                behavior: l,
                actionType: p
            });
        e.preventDefault();
        return
    }
    if (n === he.left) {
        if (!v)
            return;
        if (g)
            tp(m, !1, t),
            Yt(e.target, {
                behavior: a,
                actionType: p
            }),
            e.preventDefault();
        else {
            let b = m.parentElement.closest('[role="treeitem"]');
            b && (b.focus(),
            op(b),
            e.preventDefault())
        }
        return
    }
    if (n === he.down || n === he.up) {
        let b = n === he.down ? "following" : "preceding"
          , x = Qce(m, b);
        x && (x.focus(),
        op(x),
        e.preventDefault());
        return
    }
    if (n === he.home || n === he.end) {
        let b = n === he.home, x = m.closest(".tree"), S;
        if (v)
            S = x[b ? "firstElementChild" : "lastElementChild"].firstElementChild.closest('[role="treeitem"]'),
            eue(S) || (S = Qce(S, b ? "following" : "preceding"));
        else {
            let w = x.querySelectorAll(".tree-item.is-leaf")
              , A = b ? 0 : w.length - 1;
            S = w[A]
        }
        S.focus(),
        op(S),
        e.preventDefault();
        return
    }
    if (n === he.numPadAsterisk || n === he.eight && r) {
        if (!v)
            return;
        let b = m.closest("ul");
        for (let x = 0; x < b.children.length; x++) {
            let S = b.children.item(x);
            S.matches('[role="treeitem"][aria-expanded="false"]') && (tp(S, !0, t),
            Yt(e.target, {
                behavior: l,
                actionType: p
            }))
        }
        e.preventDefault();
        return
    }
}

//

let _a = e=>e === null || !(typeof e == "object" || typeof e == "function"),
    Zn = {},
    er = ()=>document.createComment(""),
    Il = e=>typeof e == "function" && vee.has(e);
let ow = e=>e.index !== -1;
let ew = typeof window < "u" && window.customElements != null && window.customElements.polyfillWrapFlushCallback !== void 0;
let _m = class {
    constructor(t, o, n) {
        this.__parts = [],
        this.template = t,
        this.processor = o,
        this.options = n
    }
    update(t) {
        let o = 0;
        for (let n of this.__parts)
            n !== void 0 && n.setValue(t[o]),
            o++;
        for (let n of this.__parts)
            n !== void 0 && n.commit()
    }
    _clone() {
        let t = ew ? this.template.element.content.cloneNode(!0) : document.importNode(this.template.element.content, !0), o = [], n = this.template.parts, r = document.createTreeWalker(t, 133, null, !1), s = 0, i = 0, a, l = r.nextNode();
        for (; s < n.length; ) {
            if (a = n[s],
            !ow(a)) {
                this.__parts.push(void 0),
                s++;
                continue
            }
            for (; i < a.index; )
                i++,
                l.nodeName === "TEMPLATE" && (o.push(l),
                r.currentNode = l.content),
                (l = r.nextNode()) === null && (r.currentNode = o.pop(),
                l = r.nextNode());
            if (a.type === "node") {
                let c = this.processor.handleTextExpression(this.options);
                c.insertAfterNode(l.previousSibling),
                this.__parts.push(c)
            } else
                this.__parts.push(...this.processor.handleAttributeExpressions(l, a.name, a.strings, this.options));
            s++
        }
        return ew && (document.adoptNode(t),
        customElements.upgrade(t)),
        t
    }
};
let tr = class {
    constructor(t) {
        this.value = void 0,
        this.__pendingValue = void 0,
        this.options = t
    }
    appendInto(t) {
        this.startNode = t.appendChild(er()),
        this.endNode = t.appendChild(er())
    }
    insertAfterNode(t) {
        this.startNode = t,
        this.endNode = t.nextSibling
    }
    appendIntoPart(t) {
        t.__insert(this.startNode = er()),
        t.__insert(this.endNode = er())
    }
    insertAfterPart(t) {
        t.__insert(this.startNode = er()),
        this.endNode = t.endNode,
        t.endNode = this.startNode
    }
    setValue(t) {
        this.__pendingValue = t
    }
    commit() {
        if (this.startNode.parentNode === null)
            return;
        for (; Il(this.__pendingValue); ) {
            let o = this.__pendingValue;
            this.__pendingValue = Zn,
            o(this)
        }
        let t = this.__pendingValue;
        t !== Zn && (_a(t) ? t !== this.value && this.__commitText(t) : t instanceof Ti ? this.__commitTemplateResult(t) : t instanceof Node ? this.__commitNode(t) : fb(t) ? this.__commitIterable(t) : t === va ? (this.value = va,
        this.clear()) : this.__commitText(t))
    }
    __insert(t) {
        this.endNode.parentNode.insertBefore(t, this.endNode)
    }
    __commitNode(t) {
        this.value !== t && (this.clear(),
        this.__insert(t),
        this.value = t)
    }
    __commitText(t) {
        let o = this.startNode.nextSibling;
        t = t ?? "";
        let n = typeof t == "string" ? t : String(t);
        o === this.endNode.previousSibling && o.nodeType === 3 ? o.data = n : this.__commitNode(document.createTextNode(n)),
        this.value = t
    }
    __commitTemplateResult(t) {
        let o = this.options.templateFactory(t);
        if (this.value instanceof _m && this.value.template === o)
            this.value.update(t.values);
        else {
            let n = new _m(o,t.processor,this.options)
              , r = n._clone();
            n.update(t.values),
            this.__commitNode(r),
            this.value = n
        }
    }
    __commitIterable(t) {
        Array.isArray(this.value) || (this.value = [],
        this.clear());
        let o = this.value, n = 0, r;
        for (let s of t)
            r = o[n],
            r === void 0 && (r = new tr(this.options),
            o.push(r),
            n === 0 ? r.appendIntoPart(this) : r.insertAfterPart(o[n - 1])),
            r.setValue(s),
            r.commit(),
            n++;
        n < o.length && (o.length = n,
        this.clear(r && r.endNode))
    }
    clear(t=this.startNode) {
        Ll(this.startNode.parentNode, t.nextSibling, this.endNode)
    }
};
let Ll = (e,t,o=null)=>{
    for (; t !== o; ) {
        let n = t.nextSibling;
        e.removeChild(t),
        t = n
    }
};
let iw = new WeakMap,
    y = (e,t,o)=>{
    let n = iw.get(t);
    n === void 0 && (Ll(t, t.firstChild),
    iw.set(t, n = new tr(Object.assign({
        templateFactory: sw
    }, o))),
    n.appendInto(t)),
    n.setValue(e),
    n.commit()
};
let LK ='', RK='', IK='';
let IIe = e=>e.isAnswered ? "docon-check has-text-success" : e.isRequired ? "docon-asterisk-solid has-text-danger" : "docon-location-circle"
  , LIe = e=>e.isAnswered ? IK : e.isRequired ? LK : RK;
let RIe = e=>u` <span class="icon">
<span
    class=" docon ${IIe(e)}"
    aria-label="${LIe(e)}"
    role="status"
></span>
</span>
<span>${e.title || e.id}</span>`;
let zy = class {
    hasChildren(t) {
        return !!t.children
    }
    children(t) {
        return t.children
    }
    htmlTitle(t) {
        if (t.isCategory)
            return t.title;
        let o = document.createElement("span");
        return y(RIe(t), o),
        o.innerHTML
    }
    textTitle(t) {
        return t.title || t.id
    }
    href(t) {
        return t.href.toString()
    }
    isNewSection(t) {
        return !1
    }
    isExpanded(t) {
        return !!t.children
    }
    isSelected(t) {
        return t.isSelected
    }
    setHtmlAttributes(t, o) {}
};
var FIe = new zy;
function HIe(e) {
    let t = Kce(e.treeNodes, FIe, $S);
    return t.querySelectorAll(".tree-item.is-leaf").forEach(o=>o.classList.add("has-padding-right-extra-small")),
    t.querySelectorAll("li.tree-item").forEach(o=>o.classList.add("margin-bottom-xxs")),
    t.querySelectorAll("span[data-bi-name=tree-expander]").forEach(o=>o.classList.add("font-weight-semibold")),
    t.querySelectorAll("ul.tree-group").forEach(o=>o.classList.add("margin-left-none", "margin-bottom-xxs", "has-line-height-reset")),
    t
}
function createDropdownTree (nodes) {
    return Wce(nodes, new wR(false), 'Table of contents');
}