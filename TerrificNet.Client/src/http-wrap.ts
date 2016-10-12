import * as rsvp from "es6-promise";

export function get<T>(url: string): Promise<T> {
   return execute<T>(url, null, "GET");
}

export function post<T>(url: string, data: Object, json: boolean = true): Promise<T> {
   return execute<T>(url, data, "POST", json);
}

function execute<T>(url: string, data: Object, method: string, json: boolean = true): Promise<T> {
   return new rsvp.Promise<T>((resolve, reject) => {
      var req = new XMLHttpRequest();
      req.open(method, url);
      req.setRequestHeader("Content-Type", "application/json");

      req.onload = () => {
         if (req.status === 200) {
            resolve(json ? JSON.parse(req.response) : req.response);
         } else {
            reject(Error(req.statusText));
         }
      };

      req.onerror = () => {
         reject(Error("Network Error"));
      };

      req.send(JSON.stringify(data));
   });
}