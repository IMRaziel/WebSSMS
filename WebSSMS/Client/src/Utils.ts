import $ from "jquery";

const URL_ROOT = "http://localhost:63317"

export const Http = {
  async get(url: String){
    var resp = await $.ajax({
      method: "GET",
      url: URL_ROOT + url,
      dataType: "json"
    })
    return resp;
  },
  async post(url: String, data: Object){
    var resp = await $.ajax({
      method: "POST",
      url: URL_ROOT + url,
      data: data,
      dataType: "json"
    })
    return resp;
  }
}

export function debounce(func, wait, immediate?: boolean) {
  var timeout;
  return function () {
    var context = this, args = arguments;
    var later = function () {
      timeout = null;
      if (!immediate) func.apply(context, args);
    };
    var callNow = immediate && !timeout;
    clearTimeout(timeout);
    timeout = setTimeout(later, wait);
    if (callNow) func.apply(context, args);
  };
};