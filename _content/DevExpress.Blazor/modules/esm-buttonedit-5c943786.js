function t(t,e){const n=window.getComputedStyle(t).fontSize,i=.6*Number(n.substring(0,n.length-2));return t.style.paddingRight=e.offsetWidth+i+"px",Promise.resolve("ok")}function e(t){return new Promise((e,n)=>{t||e(!1),e(0===t.selectionStart&&t.selectionEnd===(t.value?t.value.length:0))})}function n(t){t&&t.select()}const i={calculateInputRightPadding:t,isInputValueHighlighted:e,highlightInputValue:n};export default i;export{t as calculateInputRightPadding,n as highlightInputValue,e as isInputValueHighlighted};
