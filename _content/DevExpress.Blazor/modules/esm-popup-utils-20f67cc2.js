import"./esm-chunk-d81494b9.js";import{changeDom as t,setStyles as e,toggleCssClass as i,clearStyles as o,getCurrentStyleSheet as n,elementHasCssClass as r,getClassName as s}from"./esm-dom-utils-88a2c0cb.js";import{g as l}from"./esm-chunk-13e2cf5f.js";const h="\\s*matrix\\(\\s*"+[0,0,0,0,0,0].map((function(){return"(\\-?\\d+\\.?\\d*)"})).join(",\\s*")+"\\)\\s*";function a(t){let e=0;if(null!=t&&""!==t)try{const i=t.indexOf("px");i>-1&&(e=parseFloat(t.substr(0,i)))}catch(t){}return Math.ceil(e)}function f(t){const e=new RegExp(h).exec(t.transform);return e?{left:parseInt(e[5]),top:parseInt(e[6])}:{left:0,top:0}}function d(t,e,i){t.transform="matrix(1, 0, 0, 1, "+e+", "+i+")"}function c(t,e,i){const o=t.getBoundingClientRect(),n={left:e(o.left),top:e(o.top),right:i(o.right),bottom:i(o.bottom)};return n.width=n.right-n.left,n.height=n.bottom-n.top,n}function m(t){return c(t,Math.floor,Math.ceil)}function g(t){return c(t,Math.ceil,Math.floor)}class u{constructor(t,e){this.key=t,this.info=e}checkMargin(){return!0}allowScroll(){return"height"===this.info.size}canApplyToElement(t){return s(t).indexOf("dxbs-align-"+this.key)>-1}getRange(t){const e=this.getTargetBox(t)[this.info.to],i=e+this.info.sizeM*(t.elementBox[this.info.size]+(this.checkMargin()?t.margin:0));return{from:Math.min(e,i),to:Math.max(e,i),windowOverflow:0}}getTargetBox(t){return null}validate(t,e){const i=e[this.info.size];return t.windowOverflow=Math.abs(Math.min(0,t.from-i.from)+Math.min(0,i.to-t.to)),t.validTo=Math.min(t.to,i.to),t.validFrom=Math.max(t.from,i.from),0===t.windowOverflow}applyRange(t,e){e.appliedModifierKeys[this.info.size]=this.key;const i="width"===this.info.size?"left":"top",o=e.styles;let n=t.from;this.allowScroll()&&t.windowOverflow>0&&(e.limitBox.scroll.width||(e.limitBox.scroll.width=!0,e.limitBox.width.to-=l()),e.isScrollable&&(o["max-height"]=e.height-t.windowOverflow+"px",e.width+=l(),e.elementBox.width+=l(),n=t.validFrom)),o.width=e.width+"px",this.checkMargin()&&(n+=Math.max(0,this.info.sizeM)*e.margin),e.elementBox[i]+=n,d(o,e.elementBox.left,e.elementBox.top)}dockElementToTarget(t){const e=this.getRange(t);this.dockElementToTargetInternal(e,t)||this.applyRange(e,t)}dockElementToTargetInternal(t,e){}}class p extends u{constructor(t,e,i){super(t,e,i),this.oppositePointName=i||null}getTargetBox(t){return t.targetBox.outer}getOppositePoint(){return this._oppositePoint||(this._oppositePoint=x.filter(function(t){return t.key===this.oppositePointName}.bind(this))[0])}dockElementToTargetInternal(t,e){if(this.validate(t,e.limitBox))this.applyRange(t,e);else{const i=this.getOppositePoint(),o=i.getRange(e);if(!(i.validate(o,e.limitBox)||o.windowOverflow<t.windowOverflow))return!1;i.applyRange(o,e)}return!0}}class w extends u{checkMargin(){return!1}getTargetBox(t){return t.targetBox.inner}dockElementToTargetInternal(t,e){return this.validate(t,e.limitBox),!1}validate(t,e){const i=Math.min(t.from,Math.max(0,t.to-e[this.info.size].to));return i>0&&(t.from-=i,t.to-=i),super.validate(t,e)}}const x=[new p("above",{to:"top",from:"bottom",size:"height",sizeM:-1},"below"),new p("below",{to:"bottom",from:"top",size:"height",sizeM:1},"above"),new w("top-sides",{to:"top",from:"top",size:"height",sizeM:1}),new w("bottom-sides",{to:"bottom",from:"bottom",size:"height",sizeM:-1}),new p("outside-left",{to:"left",from:"right",size:"width",sizeM:-1},"outside-right"),new p("outside-right",{to:"right",from:"left",size:"width",sizeM:1},"outside-left"),new w("left-sides",{to:"left",from:"left",size:"width",sizeM:1}),new w("right-sides",{to:"right",from:"right",size:"width",sizeM:-1})];function M(i,o,s,l){return t((function(){const t=function(t,e,i){const o=n(),s=m(t),l=g(e),h=t.ownerDocument.documentElement,d={isScrollable:r(t,"dxbs-scrollable"),specifiedOffsetModifiers:x.filter((function(e){return e.canApplyToElement(t)})),margin:a(o.marginTop),width:i?Math.max(i.width,Math.ceil(t.offsetWidth)):Math.ceil(t.offsetWidth),height:Math.ceil(t.offsetHeight),appliedModifierKeys:{height:null,width:null}},c=f(o),u=t.classList.contains("visually-hidden")?l.left:s.left;var p,w,M,b;d.elementBox={left:p=c.left-u,top:w=c.top-s.top,right:p+(M=s.width),bottom:w+(b=s.height),width:M,height:b},d.targetBox={outer:m(e),inner:g(e)},d.limitBox={scroll:{width:h.clientWidth<window.innerWidth,height:h.clientHeight<window.innerHeight},width:{from:0,to:h.clientWidth},height:{from:0,to:h.clientHeight}},d.styles={};const z=(t.getAttribute("data-popup-align")||i&&i.align).split(/\s+/);return d.offsetModifiers=x.filter((function(t){return z.some((function(e){return t.key===e}))})),d}(i,o,s);for(let e=0;e<t.offsetModifiers.length;e++)t.offsetModifiers[e].dockElementToTarget(t);l(t),e(i,t.styles)}))}function b(t){r(t,"show")?(t.isDockedElementHidden&&delete t.isDockedElementHidden,o(t),i(t,"show",!1)):t.isDockedElementHidden&&delete t.isDockedElementHidden}function z(t,e,n){null!==e&&(M(t,e,{align:n},()=>{}),i(t,"show",!0),o(t))}export{b as hide,f as parseTranslateInfo,z as show,d as translatePosition};
