/// <reference path="../typings/tsd.d.ts" />

import rsvp = require("es6-promise");

export function get<T>(url: string): Promise<T> {
   return execute<T>(url, null, "GET");
}

export function post<T>(url: string, data: Object): Promise<T> {
   return execute<T>(url, data, "POST");
}

function execute<T>(url: string, data: Object, method: string): Promise<T> {
   return new rsvp.Promise<T>((resolve, reject) => {
      var req = new XMLHttpRequest();
      req.open(method, url);
      req.setRequestHeader("Content-Type", "application/json");

      req.onload = () => {
         if (req.status === 200) {
            resolve(JSON.parse(req.response));
         }
         else {
            reject(Error(req.statusText));
         }
      };

      req.onerror = () => {
         reject(Error("Network Error"));
      };

      req.send(JSON.stringify(data));
   });
}