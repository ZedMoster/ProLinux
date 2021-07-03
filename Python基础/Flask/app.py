# app.py
import base64
import random
import time
from functools import wraps
from flask import Flask, request, render_template, redirect, url_for, flash, session, jsonify, abort, make_response

app = Flask(__name__)

users = {
    "tim", ["123456"]
}


def get_token(uid):
    token = base64.b64encode(':'.join([str(uid), str(random.random()), str(time.time() + 7200)]))
    users[uid].append(token)
    return token

@app.route('/login/V1', methods=['POST', 'GET'])
def login():
    return "resp"


if __name__ == '__main__':
    app.run(debug=True)
